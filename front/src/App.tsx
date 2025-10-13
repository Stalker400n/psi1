import { useState, useEffect } from 'react';
import { Home, Users, Music, MessageSquare, Plus, Send, Star, LogOut } from 'lucide-react';
import api from './services/api';
import type { User, Song, ChatMessage, Team } from './services/api';

type ViewType = 'home' | 'create' | 'browse' | 'join' | 'team';

export default function App() {
  const [view, setView] = useState<ViewType>('home');
  const [userName, setUserName] = useState<string>('');
  const [currentTeam, setCurrentTeam] = useState<Team | null>(null);
  const [currentUser, setCurrentUser] = useState<User | null>(null);

  // Function to handle view changes
  const handleViewChange = (newView: ViewType) => {
    console.log("Changing view to:", newView);
    setView(newView);
  };

  // Function to handle team creation
  const handleTeamCreated = (team: Team, user: User) => {
    console.log("Team created:", team);
    setCurrentTeam(team);
    setCurrentUser(user);
    setView('team');
  };

  // Function to handle team joining
  const handleTeamJoined = (team: Team, user: User) => {
    console.log("Team joined:", team);
    setCurrentTeam(team);
    setCurrentUser(user);
    setView('team');
  };

  // Function to handle leaving a team
  const handleLeaveTeam = () => {
    console.log("Leaving team");
    setCurrentTeam(null);
    setCurrentUser(null);
    setView('home');
  };

  // Render name entry if no username
  if (!userName) {
    return <NameEntry onSubmit={setUserName} />;
  }

  // Render main screen if no team and view is home
  if (!currentTeam && view === 'home') {
    return (
      <MainScreen
        onCreateTeam={() => handleViewChange('create')}
        onBrowseTeams={() => handleViewChange('browse')}
        onJoinTeam={() => handleViewChange('join')}
      />
    );
  }

  // Render create team screen
  if (view === 'create') {
    return (
      <CreateTeam 
        userName={userName} 
        onBack={() => handleViewChange('home')} 
        onTeamCreated={handleTeamCreated} 
      />
    );
  }

  // Render browse teams screen
  if (view === 'browse') {
    return (
      <BrowseTeams 
        onBack={() => handleViewChange('home')} 
        onJoinTeam={handleTeamJoined} 
        userName={userName} 
      />
    );
  }

  // Render join team screen
  if (view === 'join') {
    return (
      <JoinTeam 
        userName={userName} 
        onBack={() => handleViewChange('home')} 
        onTeamJoined={handleTeamJoined} 
      />
    );
  }

  // Render team view if we have a team
  if (currentTeam && currentUser) {
    return (
      <TeamView
        team={currentTeam}
        user={currentUser}
        onLeave={handleLeaveTeam}
      />
    );
  }

  // Fallback to main screen
  return (
    <MainScreen
      onCreateTeam={() => handleViewChange('create')}
      onBrowseTeams={() => handleViewChange('browse')}
      onJoinTeam={() => handleViewChange('join')}
    />
  );
}

interface NameEntryProps {
  onSubmit: (name: string) => void;
}

function NameEntry({ onSubmit }: NameEntryProps) {
  const [name, setName] = useState<string>('');

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-white mb-2">komcon.</h1>
        <p className="text-slate-400 mb-8">Komanda Connect</p>
        <input
          type="text"
          placeholder="Enter your name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          className="px-6 py-3 bg-slate-800 text-white rounded-lg mb-4 w-80 focus:outline-none focus:ring-2 focus:ring-blue-500"
          onKeyPress={(e) => e.key === 'Enter' && name && onSubmit(name)}
        />
        <button
          onClick={() => name && onSubmit(name)}
          className="block w-80 mx-auto px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
        >
          Continue
        </button>
      </div>
    </div>
  );
}

interface MainScreenProps {
  onCreateTeam: () => void;
  onBrowseTeams: () => void;
  onJoinTeam: () => void;
}

function MainScreen({ onCreateTeam, onBrowseTeams, onJoinTeam }: MainScreenProps) {
  // Add console logs to debug button clicks
  const handleCreateClick = () => {
    console.log("Create Team button clicked");
    onCreateTeam();
  };

  const handleBrowseClick = () => {
    console.log("Browse Teams button clicked");
    onBrowseTeams();
  };

  const handleJoinClick = () => {
    console.log("Join Team button clicked");
    onJoinTeam();
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-white mb-2">komcon.</h1>
        <p className="text-slate-400 mb-12">Komanda Connect</p>
        
        <div className="space-y-4">
          <button
            onClick={handleCreateClick}
            className="w-80 px-8 py-6 bg-blue-600 text-white text-xl rounded-lg hover:bg-blue-700 transition flex items-center justify-center gap-3"
          >
            <Plus size={28} />
            Create Team
          </button>
          
          <button
            onClick={handleBrowseClick}
            className="w-80 px-8 py-6 bg-slate-800 text-white text-xl rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-3"
          >
            <Users size={28} />
            Browse Public Teams
          </button>
          
          <button
            onClick={handleJoinClick}
            className="w-80 px-8 py-6 bg-slate-800 text-white text-xl rounded-lg hover:bg-slate-700 transition flex items-center justify-center gap-3"
          >
            <Home size={28} />
            Join with Code
          </button>
        </div>
      </div>
    </div>
  );
}

interface CreateTeamProps {
  userName: string;
  onBack: () => void;
  onTeamCreated: (team: Team, user: User) => void;
}

function CreateTeam({ userName, onBack, onTeamCreated }: CreateTeamProps) {
  const [teamName, setTeamName] = useState<string>('');
  const [isPrivate, setIsPrivate] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);

  const handleCreate = async () => {
    if (!teamName) return;
    setLoading(true);
    
    try {
      const team = await api.teamsApi.create({ name: teamName, isPrivate });
      const user = await api.usersApi.add(team.id, { name: userName, score: 0, isActive: true });
      onTeamCreated(team, user);
    } catch (error) {
      console.error('Error creating team:', error);
      alert('Failed to create team');
    }
    setLoading(false);
  };

  return (
    <div className="min-h-screen bg-slate-950 p-8">
      <button onClick={onBack} className="text-slate-400 hover:text-white mb-8">← Back</button>
      
      <div className="max-w-md mx-auto">
        <h1 className="text-4xl font-bold text-white mb-8">Create Team</h1>
        
        <input
          type="text"
          placeholder="Team name"
          value={teamName}
          onChange={(e) => setTeamName(e.target.value)}
          className="w-full px-4 py-3 bg-slate-800 text-white rounded-lg mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        
        <label className="flex items-center gap-3 text-white mb-6 cursor-pointer">
          <input
            type="checkbox"
            checked={isPrivate}
            onChange={(e) => setIsPrivate(e.target.checked)}
            className="w-5 h-5"
          />
          Private Team
        </label>
        
        <button
          onClick={handleCreate}
          disabled={loading || !teamName}
          className="w-full px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-50"
        >
          {loading ? 'Creating...' : 'Create Team'}
        </button>
      </div>
    </div>
  );
}

interface BrowseTeamsProps {
  onBack: () => void;
  onJoinTeam: (team: Team, user: User) => void;
  userName: string;
}

function BrowseTeams({ onBack, onJoinTeam, userName }: BrowseTeamsProps) {
  const [teams, setTeams] = useState<Team[]>([]);
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    fetchTeams();
  }, []);

  const fetchTeams = async () => {
    try {
      const data = await api.teamsApi.getAll();
      setTeams(data.filter((t: Team) => !t.isPrivate));
    } catch (error) {
      console.error('Error fetching teams:', error);
    }
    setLoading(false);
  };

  const handleJoin = async (team: Team) => {
    try {
      const user = await api.usersApi.add(team.id, { name: userName, score: 0, isActive: true });
      onJoinTeam(team, user);
    } catch (error) {
      console.error('Error joining team:', error);
      alert('Failed to join team');
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 p-8">
      <button onClick={onBack} className="text-slate-400 hover:text-white mb-8">← Back</button>
      
      <h1 className="text-4xl font-bold text-white mb-8">Public Teams</h1>
      
      {loading ? (
        <p className="text-slate-400">Loading...</p>
      ) : teams.length === 0 ? (
        <p className="text-slate-400">No public teams available</p>
      ) : (
        <div className="grid gap-4 max-w-2xl">
          {teams.map(team => (
            <div key={team.id} className="bg-slate-800 p-6 rounded-lg flex justify-between items-center">
              <div>
                <h3 className="text-xl text-white font-semibold">{team.name}</h3>
                <p className="text-slate-400 text-sm">Code: {team.id}</p>
              </div>
              <button
                onClick={() => handleJoin(team)}
                className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
              >
                Join
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

interface JoinTeamProps {
  userName: string;
  onBack: () => void;
  onTeamJoined: (team: Team, user: User) => void;
}

function JoinTeam({ userName, onBack, onTeamJoined }: JoinTeamProps) {
  const [code, setCode] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);

  const handleJoin = async () => {
    if (!code) return;
    setLoading(true);

    try {
      const team = await api.teamsApi.getById(parseInt(code));
      const user = await api.usersApi.add(team.id, { name: userName, score: 0, isActive: true });
      onTeamJoined(team, user);
    } catch (error) {
      console.error('Error joining team:', error);
      alert('Failed to join team. Check the code.');
    }
    setLoading(false);
  };

  return (
    <div className="min-h-screen bg-slate-950 p-8">
      <button onClick={onBack} className="text-slate-400 hover:text-white mb-8">← Back</button>
      
      <div className="max-w-md mx-auto">
        <h1 className="text-4xl font-bold text-white mb-8">Join Team</h1>
        
        <input
          type="text"
          placeholder="Enter team code"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          className="w-full px-4 py-3 bg-slate-800 text-white rounded-lg mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500"
          onKeyPress={(e) => e.key === 'Enter' && handleJoin()}
        />
        
        <button
          onClick={handleJoin}
          disabled={loading || !code}
          className="w-full px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-50"
        >
          {loading ? 'Joining...' : 'Join Team'}
        </button>
      </div>
    </div>
  );
}

interface TeamViewProps {
  team: Team;
  user: User;
  onLeave: () => void;
}

function TeamView({ team, user, onLeave }: TeamViewProps) {
  const [view, setView] = useState<'playlist' | 'chat' | 'leaderboard'>('playlist');
  
  return (
    <div className="min-h-screen bg-slate-950">
      <div className="bg-slate-900 border-b border-slate-800 p-4">
        <div className="max-w-7xl mx-auto flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-white">{team.name}</h1>
            <p className="text-slate-400 text-sm">Code: {team.id} • {user.name}</p>
          </div>
          <button
            onClick={onLeave}
            className="px-4 py-2 bg-slate-800 text-white rounded-lg hover:bg-slate-700 transition flex items-center gap-2"
          >
            <LogOut size={18} />
            Leave
          </button>
        </div>
      </div>

      <div className="max-w-7xl mx-auto p-4">
        <div className="flex gap-2 mb-6">
          <button
            onClick={() => setView('playlist')}
            className={`px-6 py-2 rounded-lg transition ${view === 'playlist' ? 'bg-blue-600 text-white' : 'bg-slate-800 text-slate-400'}`}
          >
            <Music size={18} className="inline mr-2" />
            Playlist
          </button>
          <button
            onClick={() => setView('chat')}
            className={`px-6 py-2 rounded-lg transition ${view === 'chat' ? 'bg-blue-600 text-white' : 'bg-slate-800 text-slate-400'}`}
          >
            <MessageSquare size={18} className="inline mr-2" />
            Chat
          </button>
          <button
            onClick={() => setView('leaderboard')}
            className={`px-6 py-2 rounded-lg transition ${view === 'leaderboard' ? 'bg-blue-600 text-white' : 'bg-slate-800 text-slate-400'}`}
          >
            <Star size={18} className="inline mr-2" />
            Leaderboard
          </button>
        </div>

        {view === 'playlist' && <PlaylistView teamId={team.id} userId={user.id} />}
        {view === 'chat' && <ChatView teamId={team.id} userName={user.name} />}
        {view === 'leaderboard' && <LeaderboardView teamId={team.id} />}
      </div>
    </div>
  );
}

interface PlaylistViewProps {
  teamId: number;
  userId: number;
}

function PlaylistView({ teamId, userId }: PlaylistViewProps) {
  const [songs, setSongs] = useState<Song[]>([]);
  const [showAdd, setShowAdd] = useState<boolean>(false);
  const [newSong, setNewSong] = useState({ link: '', title: '', artist: '' });
  const [userName, setUserName] = useState<string>('');

  useEffect(() => {
    fetchSongs();
    fetchUserName();
    const interval = setInterval(fetchSongs, 3000);
    return () => clearInterval(interval);
  }, [teamId]);

  const fetchUserName = async () => {
    try {
      const user = await api.usersApi.getById(teamId, userId);
      setUserName(user.name);
    } catch (error) {
      console.error('Error fetching user:', error);
    }
  };

  const fetchSongs = async () => {
    try {
      const data = await api.songsApi.getAll(teamId);
      setSongs(data);
    } catch (error) {
      console.error('Error fetching songs:', error);
    }
  };

  const addSong = async () => {
    if (!newSong.link || !newSong.title) return;

    try {
      await api.songsApi.add(teamId, { 
        ...newSong, 
        rating: 0,
        addedByUserId: userId,
        addedByUserName: userName
      });
      setNewSong({ link: '', title: '', artist: '' });
      setShowAdd(false);
      fetchSongs();
    } catch (error) {
      console.error('Error adding song:', error);
    }
  };

  const currentSong = songs[0];

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div>
        <div className="bg-slate-900 rounded-lg p-6 mb-6">
          <h2 className="text-xl font-semibold text-white mb-4">Now Playing</h2>
          {currentSong ? (
            <div>
              <div className="bg-black aspect-video rounded mb-4 flex items-center justify-center">
                <iframe
                  width="100%"
                  height="100%"
                  src={`https://www.youtube.com/embed/${extractYoutubeId(currentSong.link)}`}
                  frameBorder="0"
                  allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                  allowFullScreen
                  className="rounded"
                />
              </div>
              <h3 className="text-white font-semibold">{currentSong.title}</h3>
              <p className="text-slate-400">{currentSong.artist}</p>
            </div>
          ) : (
            <p className="text-slate-400">No songs in queue</p>
          )}
        </div>

        <button
          onClick={() => setShowAdd(!showAdd)}
          className="w-full px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition flex items-center justify-center gap-2"
        >
          <Plus size={20} />
          Add Song to Queue
        </button>

        {showAdd && (
          <div className="mt-4 bg-slate-900 rounded-lg p-6">
            <input
              type="text"
              placeholder="YouTube Link"
              value={newSong.link}
              onChange={(e) => setNewSong({ ...newSong, link: e.target.value })}
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="text"
              placeholder="Song Title"
              value={newSong.title}
              onChange={(e) => setNewSong({ ...newSong, title: e.target.value })}
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <input
              type="text"
              placeholder="Artist"
              value={newSong.artist}
              onChange={(e) => setNewSong({ ...newSong, artist: e.target.value })}
              className="w-full px-4 py-2 bg-slate-800 text-white rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button
              onClick={addSong}
              className="w-full px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition"
            >
              Add
            </button>
          </div>
        )}
      </div>

      <div className="bg-slate-900 rounded-lg p-6">
        <h2 className="text-xl font-semibold text-white mb-4">Queue</h2>
        <div className="space-y-3">
          {songs.slice(1).map((song, idx) => (
            <div key={song.id} className="bg-slate-800 p-4 rounded-lg">
              <div className="flex justify-between items-start">
                <div>
                  <p className="text-slate-400 text-sm">#{idx + 2}</p>
                  <h4 className="text-white font-semibold">{song.title}</h4>
                  <p className="text-slate-400 text-sm">{song.artist}</p>
                </div>
                <div className="flex items-center gap-1">
                  <Star size={16} className="text-yellow-500" />
                  <span className="text-white">{song.rating || 0}</span>
                </div>
              </div>
            </div>
          ))}
          {songs.length <= 1 && <p className="text-slate-400">No songs in queue</p>}
        </div>
      </div>
    </div>
  );
}

interface ChatViewProps {
  teamId: number;
  userName: string;
}

function ChatView({ teamId, userName }: ChatViewProps) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [newMessage, setNewMessage] = useState<string>('');

  useEffect(() => {
    fetchMessages();
    const interval = setInterval(fetchMessages, 2000);
    return () => clearInterval(interval);
  }, [teamId]);

  const fetchMessages = async () => {
    try {
      const data = await api.chatsApi.getAll(teamId);
      setMessages(data);
    } catch (error) {
      console.error('Error fetching messages:', error);
    }
  };

  const sendMessage = async () => {
    if (!newMessage.trim()) return;

    try {
      await api.chatsApi.add(teamId, { text: newMessage, userName: userName });
      setNewMessage('');
      fetchMessages();
    } catch (error) {
      console.error('Error sending message:', error);
    }
  };

  return (
    <div className="bg-slate-900 rounded-lg p-6 h-[600px] flex flex-col">
      <h2 className="text-xl font-semibold text-white mb-4">Chat</h2>
      
      <div className="flex-1 overflow-y-auto space-y-3 mb-4">
        {messages.map((msg) => (
          <div key={msg.id} className="bg-slate-800 p-3 rounded-lg">
            <p className="text-blue-400 text-sm font-semibold">{msg.userName}</p>
            <p className="text-white">{msg.text}</p>
            <p className="text-slate-500 text-xs mt-1">{new Date(msg.timestamp).toLocaleTimeString()}</p>
          </div>
        ))}
      </div>

      <div className="flex gap-2">
        <input
          type="text"
          placeholder="Type a message..."
          value={newMessage}
          onChange={(e) => setNewMessage(e.target.value)}
          onKeyPress={(e) => e.key === 'Enter' && sendMessage()}
          className="flex-1 px-4 py-2 bg-slate-800 text-white rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <button
          onClick={sendMessage}
          className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
        >
          <Send size={20} />
        </button>
      </div>
    </div>
  );
}

interface LeaderboardViewProps {
  teamId: number;
}

function LeaderboardView({ teamId }: LeaderboardViewProps) {
  const [users, setUsers] = useState<User[]>([]);

  useEffect(() => {
    fetchUsers();
    const interval = setInterval(fetchUsers, 3000);
    return () => clearInterval(interval);
  }, [teamId]);

  const fetchUsers = async () => {
    try {
      const data = await api.usersApi.getAll(teamId);
      setUsers(data.sort((a: User, b: User) => b.score - a.score));
    } catch (error) {
      console.error('Error fetching users:', error);
    }
  };

  return (
    <div className="bg-slate-900 rounded-lg p-6">
      <h2 className="text-xl font-semibold text-white mb-4">Leaderboard</h2>
      <div className="space-y-3">
        {users.map((user, idx) => (
          <div key={user.id} className="bg-slate-800 p-4 rounded-lg flex justify-between items-center">
            <div className="flex items-center gap-4">
              <span className="text-2xl font-bold text-slate-600">#{idx + 1}</span>
              <div>
                <p className="text-white font-semibold">{user.name}</p>
                <p className="text-slate-400 text-sm">{user.isActive ? 'Active' : 'Inactive'}</p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <Star size={20} className="text-yellow-500" />
              <span className="text-white text-xl font-bold">{user.score}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function extractYoutubeId(url: string): string {
  const match = url.match(/(?:youtube\.com\/(?:[^/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?/\s]{11})/);
  return match ? match[1] : '';
}
