import { useState, useEffect, useRef, useCallback } from "react";
import { SkipForward, SkipBack, Pause, Play } from "lucide-react";
import api from "../services/api.service";
import type { Song, User } from "../services/api.service";
import { extractYoutubeId } from "../utils/youtube.utils";
import { HeatMeter } from "../components/heat-meter.component";
import { RightPanel } from "../components/right-panel.component";
import { useToast } from "../contexts/toast-context";
import * as signalR from "@microsoft/signalr";

interface PlaylistPageProps {
  teamId: number;
  userId: number;
  userName: string;
}

interface PlaybackState {
  CurrentSongIndex: number;     
  IsPlaying: boolean;           
  StartedAtUtc?: string;        
  ElapsedSeconds: number;       
}

export function PlaylistPage({ teamId, userId, userName }: PlaylistPageProps) {
  const { showToast } = useToast();
  const iframeRef = useRef<HTMLIFrameElement>(null);
  const [queue, setQueue] = useState<Song[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [currentSong, setCurrentSong] = useState<Song | null>(null);
  const [currentRating, setCurrentRating] = useState<number>(0);
  const [currentIndex, setCurrentIndex] = useState<number>(0);
  const [isPlaying, setIsPlaying] = useState<boolean>(true);
  const [playerReady, setPlayerReady] = useState<boolean>(false);
  const [lastState, setLastState] = useState<PlaybackState | null>(null);
  const [currentTime, setCurrentTime] = useState<number>(0);
  const hubConnection = useRef<signalR.HubConnection | null>(null);
  const currentTimeRef = useRef<number>(0);
  const playerReadyRef = useRef<boolean>(false);

  // Update refs when state changes
  useEffect(() => {
    currentTimeRef.current = currentTime;
  }, [currentTime]);

  useEffect(() => {
    playerReadyRef.current = playerReady;
  }, [playerReady]);

  // Post messages to YouTube iframe
  const post = useCallback((func: string, args: any[] = []) => {
    iframeRef.current?.contentWindow?.postMessage(
      JSON.stringify({ event: "command", func, args }),
      "*"
    );
  }, []);

  // Get current time from player
  const getCurrentTime = useCallback(() => {
    return currentTimeRef.current;
  }, []);

  // Compute expected playback position
  const computeExpected = useCallback((state: PlaybackState) => {
    return (
      state.ElapsedSeconds +
      (state.StartedAtUtc
        ? (Date.now() - new Date(state.StartedAtUtc).getTime()) / 1000
        : 0)
    );
  }, []);

  useEffect(() => {
    fetchQueueAndCurrent();
    fetchUsers();

    // Setup SignalR
    hubConnection.current = new signalR.HubConnectionBuilder()
      .withUrl("https://localhost:7130/teamHub", { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Debug)
      .build();

    hubConnection.current
      .start()
      .then(() => {
        console.log("✅ SignalR connected. Connection ID:", hubConnection.current?.connectionId);
        console.log("Connection State:", hubConnection.current?.state);
        return hubConnection.current?.invoke("JoinTeam", teamId.toString());
      })
      .then(() => {
        console.log("✅ JoinTeam invoked successfully");
      })
      .catch((err) => console.error("❌ SignalR connection error: ", err));

    hubConnection.current.on("PlaybackState", (state: any) => {
      const timestamp = new Date().toISOString();
      console.log(`[${timestamp}] 🎯 Received PlaybackState (RAW):`, JSON.stringify(state, null, 2));
      console.log("State keys:", Object.keys(state));
      console.log("State values:", Object.values(state));
      
      // Handle both camelCase (from SignalR conversion) and PascalCase
      const normalizedState: PlaybackState = {
        CurrentSongIndex: state.currentSongIndex ?? state.CurrentSongIndex ?? 0,
        IsPlaying: state.isPlaying ?? state.IsPlaying ?? false,
        StartedAtUtc: state.startedAtUtc ?? state.StartedAtUtc,
        ElapsedSeconds: state.elapsedSeconds ?? state.ElapsedSeconds ?? 0
      };
      
      console.log("✅ Normalized state:", JSON.stringify(normalizedState, null, 2));
      console.log("IsPlaying value:", normalizedState.IsPlaying, "type:", typeof normalizedState.IsPlaying);
      
      setLastState(normalizedState);
      setIsPlaying(normalizedState.IsPlaying);
      console.log("State set in React - isPlaying now:", normalizedState.IsPlaying);
      
      const player = iframeRef.current;

      if (!player || !playerReadyRef.current) {
        console.log("⏸️ Player not ready yet, skipping playback control");
        return;
      }

      const expected = computeExpected(normalizedState);

      if (normalizedState.IsPlaying) {
        console.log("▶️ Sending playVideo command to YouTube");
        post("playVideo");
        
        const actual = getCurrentTime();
        console.log("Current time:", actual, "Expected:", expected, "Diff:", Math.abs(actual - expected));
        if (Math.abs(actual - expected) > 2) {
          console.log("⏩ Seeking to", expected);
          post("seekTo", [expected]);
        }
      } else {
        console.log("⏸️ Sending pauseVideo command to YouTube");
        post("pauseVideo");
      }
    });

    const handleMessage = (event: MessageEvent) => {
      if (event.origin !== "https://www.youtube.com") {
        // console.log("Ignoring message from:", event.origin);
        return;
      }
      
      console.log("📩 Message from YouTube:", event.data);
      
      try {
        const data = JSON.parse(event.data);
        console.log("Parsed YouTube data:", data);
        
        if (data.event === "onReady") {
          console.log("🎬 YouTube player ready!");
          setPlayerReady(true);
          playerReadyRef.current = true;
        } else if (data.info && data.info.currentTime !== undefined) {
          const time = data.info.currentTime;
          setCurrentTime(time);
          currentTimeRef.current = time;
        }
      } catch (e) {
        console.log("Non-JSON message from YouTube, ignoring");
      }
    };

    window.addEventListener("message", handleMessage);

    return () => {
      window.removeEventListener("message", handleMessage);
      hubConnection.current
        ?.invoke("LeaveTeam", teamId.toString())
        .then(() => {
          hubConnection.current?.stop();
        });
    };
  }, [teamId]);

  // Periodic sync check
  useEffect(() => {
    if (!playerReady || !lastState) return;

    const interval = setInterval(() => {
      if (!lastState?.IsPlaying) return;

      const expected = computeExpected(lastState);
      const actual = getCurrentTime();

      if (Math.abs(actual - expected) > 2) {
        post("seekTo", [expected]);
      }
    }, 10000);

    return () => clearInterval(interval);
  }, [playerReady, lastState, post, getCurrentTime, computeExpected]);

  // Request current time periodically
  useEffect(() => {
    if (!playerReady) return;

    const interval = setInterval(() => {
      post("getCurrentTime");
    }, 1000);

    return () => clearInterval(interval);
  }, [playerReady, post]);

  useEffect(() => {
    if (currentSong) {
      fetchCurrentRating();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentSong?.id, userId]);

  const fetchCurrentRating = async () => {
    if (!currentSong) return;

    try {
      const userRating = await api.ratingsApi.getUserRating(
        teamId,
        currentSong.id,
        userId
      );
      setCurrentRating(userRating?.rating || 0);
    } catch (error) {
      console.error("Error fetching current rating:", error);
      setCurrentRating(0);
    }
  };

  const fetchUsers = async () => {
    try {
      const userData = await api.usersApi.getAll(teamId);
      setUsers(userData);
    } catch (error) {
      console.error("Error fetching users:", error);
      setUsers([]);
    }
  };

  const fetchQueueAndCurrent = async () => {
    try {
      const historyData = await api.teamsApi.getQueueHistory(teamId);
      setQueue(historyData.songs);
      setCurrentIndex(historyData.currentIndex);

      const current = historyData.songs.find(
        (s) => s.index === historyData.currentIndex
      );
      setCurrentSong(current || null);
    } catch (error) {
      console.error("Error fetching queue history:", error);
      setQueue([]);
      setCurrentSong(null);
      setCurrentIndex(0);
    }
  };

  const handleRatingSubmit = async (rating: number) => {
    if (!currentSong) return;

    try {
      await api.ratingsApi.submitRating(teamId, currentSong.id, userId, rating);
      setCurrentRating(rating);
    } catch (error) {
      console.error("Error submitting rating:", error);
      showToast("Failed to submit rating", "error");
    }
  };

  const addSong = async (url: string, addToBeginning: boolean) => {
    if (!url.trim()) return;

    try {
      await api.songsApi.add(
        teamId,
        {
          link: url,
          title: "Song Name",
          artist: "Artist",
          rating: 0,
          addedByUserId: userId,
          addedByUserName: userName,
        },
        addToBeginning
      );
      fetchQueueAndCurrent();
    } catch (error) {
      console.error("Error adding song:", error);
      const errorMessage =
        error instanceof Error ? error.message : "Failed to add song";
      throw new Error(errorMessage);
    }
  };

  const deleteSong = async (songId: number) => {
    try {
      await api.songsApi.delete(teamId, songId);
      fetchQueueAndCurrent();
      showToast("Song removed from queue", "success");
    } catch (error) {
      console.error("Error deleting song:", error);
      showToast("Failed to remove song", "error");
    }
  };

  const nextSong = async () => {
    const currentUser = users.find((u) => u.id === userId);
    const canControlPlayback =
      currentUser?.role === "Moderator" || currentUser?.role === "Owner";

    if (!canControlPlayback) {
      showToast("Only Moderators and Owners can skip songs", "warning");
      return;
    }

    try {
      await api.songsApi.next(teamId);
      fetchQueueAndCurrent();
    } catch (error) {
      console.error("Error skipping to next song:", error);
      showToast("No more songs in queue", "info");
    }
  };

  const previousSong = async () => {
    const currentUser = users.find((u) => u.id === userId);
    const canControlPlayback =
      currentUser?.role === "Moderator" || currentUser?.role === "Owner";

    if (!canControlPlayback) {
      showToast(
        "Only Moderators and Owners can play previous songs",
        "warning"
      );
      return;
    }

    try {
      await api.songsApi.previous(teamId);
      fetchQueueAndCurrent();
    } catch (error) {
      console.error("Error going to previous song:", error);
      showToast("Already at first song", "info");
    }
  };

  const togglePlayPause = async () => {
    const currentUser = users.find((u) => u.id === userId);
    const canControlPlayback = 
      currentUser?.role === "Moderator" || currentUser?.role === "Owner";

    if (!canControlPlayback) {
      showToast("Only Moderators and Owners can pause/play the video", "warning");
      return;
    }

    console.log("=== TOGGLE PLAY/PAUSE DEBUG ===");
    console.log("Hub connection exists:", !!hubConnection.current);
    console.log("Connection state:", hubConnection.current?.state);
    console.log("Connection ID:", hubConnection.current?.connectionId);
    console.log("Team ID:", teamId.toString());
    console.log("Is playing:", isPlaying);
    console.log("Last state:", lastState);

    if (!hubConnection.current) {
      console.error("❌ Hub connection is null!");
      return;
    }

    if (hubConnection.current.state !== signalR.HubConnectionState.Connected) {
      console.error("❌ Connection not in Connected state:", hubConnection.current.state);
      showToast("Connection error. Please refresh.", "error");
      return;
    }

    const newPlayingState = !isPlaying;
    const methodName = newPlayingState ? "Play" : "Pause";

    console.log(`🎬 Invoking ${methodName} for team ${teamId}...`);

    try {
      const result = await hubConnection.current.invoke(methodName, teamId.toString());
      console.log(`✅ ${methodName} invoke returned:`, result);
    } catch (error) {
      console.error(`❌ ${methodName} failed:`, error);
      console.error("Error type:", error?.constructor?.name);
      console.error("Error message:", error?.message);
      showToast("Warning: Playback not synced with other users", "warning");
    }
  };

  return (
    <div className="flex gap-4 h-[calc(100vh-200px)]">
      <div className="w-20 flex-shrink-0">
        {currentSong && (
          <HeatMeter
            currentRating={currentRating}
            onSubmit={handleRatingSubmit}
          />
        )}
      </div>

      <div className="flex-1 flex flex-col gap-4">
        <div className="bg-slate-900 rounded-lg p-6 flex-1">
          <h2 className="text-xl font-semibold text-white mb-4">Now Playing</h2>
          {currentSong ? (
            <div className="h-full flex flex-col">
              <div className="bg-black aspect-video rounded mb-4 flex items-center justify-center flex-shrink-0 relative">
                <iframe
                  ref={iframeRef}
                  width="100%"
                  height="100%"
                  src={`https://www.youtube.com/embed/${extractYoutubeId(
                    currentSong.link
                  )}?enablejsapi=1&controls=0&modestbranding=1&rel=0&origin=${
                    window.location.origin
                  }`}
                  frameBorder="0"
                  allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                  allowFullScreen
                  className="rounded"
                  onLoad={() => {
                    console.log("🎥 iFrame loaded, requesting YouTube API ready state");
                    // Give YouTube a moment to initialize, then check if player is ready
                    setTimeout(() => {
                      if (iframeRef.current) {
                        iframeRef.current.contentWindow?.postMessage(
                          '{"event":"listening"}',
                          "*"
                        );
                      }
                    }, 100);
                  }}
                />
                <div className="absolute inset-0 rounded pointer-events-none" />
              </div>

              <div className="mb-4">
                <h3 className="text-white font-semibold text-lg">
                  {currentSong.title}
                </h3>
                <p className="text-slate-400">{currentSong.artist}</p>
                <p className="text-slate-500 text-sm">
                  Added by {currentSong.addedByUserName}
                </p>
                <p className="text-yellow-400 text-xs mt-1">
                  Position: #{currentSong.index + 1}
                </p>
              </div>

              <div className="flex gap-2 mt-auto">
                <button
                  onClick={previousSong}
                  className="flex-1 px-4 py-3 bg-slate-700 text-white rounded-lg hover:bg-slate-600 transition flex items-center justify-center gap-2 font-semibold"
                >
                  <SkipBack size={18} />
                  Previous
                </button>
                <button
                  onClick={togglePlayPause}
                  disabled={
                    users.find((u) => u.id === userId)?.role !== "Moderator" &&
                    users.find((u) => u.id === userId)?.role !== "Owner"
                  }
                  className="flex-1 px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-500 disabled:bg-slate-500 disabled:cursor-not-allowed transition flex items-center justify-center gap-2 font-semibold"
                >
                  {isPlaying ? <Pause size={18} /> : <Play size={18} />}
                  {isPlaying ? "Pause" : "Play"}
                </button>
                <button
                  onClick={nextSong}
                  className="flex-1 px-4 py-3 bg-yellow-500 text-black rounded-lg hover:bg-yellow-400 transition flex items-center justify-center gap-2 font-semibold"
                >
                  <SkipForward size={18} />
                  Next
                </button>
              </div>
            </div>
          ) : (
            <div className="h-full flex items-center justify-center">
              <p className="text-slate-400">No songs in queue</p>
            </div>
          )}
        </div>
      </div>

      <div className="w-96 flex-shrink-0">
        <RightPanel
          teamId={teamId}
          userId={userId}
          userName={userName}
          queue={queue}
          currentIndex={currentIndex}
          users={users}
          userRole={users.find((u) => u.id === userId)?.role || "Member"}
          onDeleteSong={deleteSong}
          onAddSong={addSong}
        />
      </div>
    </div>
  );
}