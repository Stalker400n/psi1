export interface User {
  id: number;
  name: string;
  score: number;
  joinedAt: string;
  isActive: boolean;
  role: 'Member' | 'Moderator' | 'Owner';
}

export interface GlobalUser {
  id: number;
  name: string;
  isNew: boolean;
}

export interface LoginRequest {
  name: string;
  deviceFingerprint: string;
  deviceInfo?: string;
}

export interface Song {
  id: number;
  link: string;
  title: string;
  artist: string;
  rating: number;
  addedByUserId: number;
  addedByUserName: string;
  addedAt: string;
  index: number;
}

export interface SongRating {
  id: number;
  songId: number;
  userId: number;
  rating: number;
  createdAt: string;
  updatedAt: string;
}

export interface ChatMessage {
  id: number;
  userName: string;
  text: string;
  timestamp: string;
}

export interface Team {
  id: number;
  name: string;
  isPrivate: boolean;
  inviteCode: string;
  createdAt: string;
  createdByUserId: number;
  currentSongIndex: number;
  songs: Song[];
  users: User[];
  messages: ChatMessage[];
}

const API_BASE = 'https://localhost:7130';

const fetchOptions = {
  headers: { 'Content-Type': 'application/json' },
  mode: 'cors' as RequestMode,
  credentials: 'omit' as RequestCredentials,
};

const teamsApi = {
  getAll: async (): Promise<Team[]> => {
    const response = await fetch(`${API_BASE}/teams`, {
      ...fetchOptions,
      method: 'GET',
    });
    if (!response.ok) throw new Error('Failed to fetch teams');
    return response.json();
  },

  getById: async (id: number): Promise<Team> => {
    const response = await fetch(`${API_BASE}/teams/${id}`, {
      ...fetchOptions,
      method: 'GET',
    });
    if (!response.ok) throw new Error('Team not found');
    return response.json();
  },

  create: async (team: { name: string; isPrivate: boolean }): Promise<Team> => {
    const response = await fetch(`${API_BASE}/teams`, {
      ...fetchOptions,
      method: 'POST',
      body: JSON.stringify(team),
    });
    if (!response.ok) throw new Error('Failed to create team');
    return response.json();
  },

  update: async (
    id: number,
    team: { name: string; isPrivate: boolean }
  ): Promise<Team> => {
    const response = await fetch(`${API_BASE}/teams/${id}`, {
      ...fetchOptions,
      method: 'PUT',
      body: JSON.stringify(team),
    });
    if (!response.ok) throw new Error('Failed to update team');
    return response.json();
  },

  delete: async (id: number) => {
    const response = await fetch(`${API_BASE}/teams/${id}`, {
      ...fetchOptions,
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete team');
    return true;
  },
};

const usersApi = {
  getAll: async (teamId: number): Promise<User[]> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/users`, {
      ...fetchOptions,
      method: 'GET',
    });
    if (!response.ok) throw new Error('Failed to fetch users');
    return response.json();
  },

  getById: async (teamId: number, userId: number): Promise<User> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/users/${userId}`,
      {
        ...fetchOptions,
        method: 'GET',
      }
    );
    if (!response.ok) throw new Error('User not found');
    return response.json();
  },

  add: async (
    teamId: number,
    user: { name: string; score: number; isActive: boolean }
  ): Promise<User> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/users`, {
      ...fetchOptions,
      method: 'POST',
      body: JSON.stringify(user),
    });
    if (!response.ok) throw new Error('Failed to add user');
    return response.json();
  },

  update: async (
    teamId: number,
    userId: number,
    user: {
      name: string;
      score: number;
      isActive: boolean;
      role?: 'Member' | 'Moderator' | 'Owner';
    }
  ): Promise<User> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/users/${userId}`,
      {
        ...fetchOptions,
        method: 'PUT',
        body: JSON.stringify(user),
      }
    );
    if (!response.ok) throw new Error('Failed to update user');
    return response.json();
  },

  delete: async (teamId: number, userId: number) => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/users/${userId}`,
      {
        ...fetchOptions,
        method: 'DELETE',
      }
    );
    if (!response.ok) throw new Error('Failed to delete user');
    return true;
  },

  changeRole: async (
    teamId: number,
    userId: number,
    role: 'Member' | 'Moderator' | 'Owner',
    requestingUserId: number
  ): Promise<User> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/users/${userId}/role`,
      {
        ...fetchOptions,
        method: 'PUT',
        body: JSON.stringify({ role, requestingUserId }),
      }
    );
    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to change role' }));
      throw new Error(error.message || 'Failed to change role');
    }
    return response.json();
  },
};

const songsApi = {
  getAll: async (teamId: number): Promise<Song[]> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs`, {
      ...fetchOptions,
      method: 'GET',
    });
    if (!response.ok) throw new Error('Failed to fetch songs');
    return response.json();
  },

  getQueue: async (teamId: number): Promise<Song[]> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs/queue`, {
      ...fetchOptions,
      method: 'GET',
    });
    if (!response.ok) throw new Error('Failed to fetch queue');
    return response.json();
  },

  getCurrent: async (teamId: number): Promise<Song> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs/current`, {
      ...fetchOptions,
      method: 'GET',
    });
    if (!response.ok) throw new Error('No current song');
    return response.json();
  },

  next: async (teamId: number): Promise<Song> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs/next`, {
      ...fetchOptions,
      method: 'POST',
    });
    if (!response.ok) throw new Error('No next song');
    return response.json();
  },

  previous: async (teamId: number): Promise<Song> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs/previous`, {
      ...fetchOptions,
      method: 'POST',
    });
    if (!response.ok) throw new Error('No previous song');
    return response.json();
  },

  jumpTo: async (teamId: number, index: number): Promise<Song> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/songs/jump/${index}`,
      {
        ...fetchOptions,
        method: 'POST',
      }
    );
    if (!response.ok) throw new Error('Failed to jump to song');
    return response.json();
  },

  getById: async (teamId: number, songId: number): Promise<Song> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/songs/${songId}`,
      {
        ...fetchOptions,
        method: 'GET',
      }
    );
    if (!response.ok) throw new Error('Song not found');
    return response.json();
  },

  add: async (
    teamId: number,
    song: {
      link: string;
      title: string;
      artist: string;
      rating?: number;
      addedByUserId: number;
      addedByUserName: string;
    },
    insertAfterCurrent: boolean = false
  ): Promise<Song> => {
    const songData = {
      link: song.link,
      title: song.title,
      artist: song.artist,
      rating: song.rating || 0,
      addedByUserId: song.addedByUserId,
      addedByUserName: song.addedByUserName,
    };

    const url = `${API_BASE}/teams/${teamId}/songs${
      insertAfterCurrent ? '?insertAfterCurrent=true' : ''
    }`;
    const response = await fetch(url, {
      ...fetchOptions,
      method: 'POST',
      body: JSON.stringify(songData),
    });

    if (!response.ok) {
      // Try to parse JSON response for structured error message
      const text = await response.text();
      let errorMessage = 'Failed to add song';
      
      try {
        const errorData = JSON.parse(text);
        if (errorData.message) {
          errorMessage = errorData.message;
        }
      } catch {
        // If not valid JSON, use the text as error message if it's not empty
        if (text.trim()) {
          errorMessage = text;
        }
      }
      
      throw new Error(errorMessage);
    }

    return response.json();
  },

  update: async (
    teamId: number,
    songId: number,
    song: {
      link: string;
      title: string;
      artist: string;
      rating: number;
    }
  ): Promise<Song> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/songs/${songId}`,
      {
        ...fetchOptions,
        method: 'PUT',
        body: JSON.stringify(song),
      }
    );
    if (!response.ok) throw new Error('Failed to update song');
    return response.json();
  },

  delete: async (teamId: number, songId: number) => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/songs/${songId}`,
      {
        ...fetchOptions,
        method: 'DELETE',
      }
    );
    if (!response.ok) throw new Error('Failed to delete song');
    return true;
  },
};

const ratingsApi = {
  // Get all ratings for a specific song
  getSongRatings: async (
    teamId: number,
    songId: number
  ): Promise<SongRating[]> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/songs/${songId}/ratings`,
      {
        ...fetchOptions,
        method: 'GET',
      }
    );
    if (!response.ok) throw new Error('Failed to fetch ratings');
    return response.json();
  },

  // Submit or update a rating
  submitRating: async (
    teamId: number,
    songId: number,
    userId: number,
    rating: number
  ): Promise<SongRating> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/songs/${songId}/ratings`,
      {
        ...fetchOptions,
        method: 'POST',
        body: JSON.stringify({ userId, rating }),
      }
    );
    if (!response.ok) throw new Error('Failed to submit rating');
    return response.json();
  },

  // Get user's rating for a specific song
  getUserRating: async (
    teamId: number,
    songId: number,
    userId: number
  ): Promise<SongRating | null> => {
    try {
      const ratings = await ratingsApi.getSongRatings(teamId, songId);
      return ratings.find((r) => r.userId === userId) || null;
    } catch {
      return null;
    }
  },
};

const chatsApi = {
  getAll: async (teamId: number): Promise<ChatMessage[]> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/chats`, {
      ...fetchOptions,
      method: 'GET',
    });
    if (!response.ok) throw new Error('Failed to fetch messages');
    return response.json();
  },

  getById: async (teamId: number, messageId: number): Promise<ChatMessage> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/chats/${messageId}`,
      {
        ...fetchOptions,
        method: 'GET',
      }
    );
    if (!response.ok) throw new Error('Message not found');
    return response.json();
  },

  add: async (
    teamId: number,
    message: { userName: string; text: string }
  ): Promise<ChatMessage> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/chats`, {
      ...fetchOptions,
      method: 'POST',
      body: JSON.stringify(message),
    });
    if (!response.ok) throw new Error('Failed to add message');
    return response.json();
  },

  update: async (
    teamId: number,
    messageId: number,
    message: { text: string }
  ): Promise<ChatMessage> => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/chats/${messageId}`,
      {
        ...fetchOptions,
        method: 'PUT',
        body: JSON.stringify(message),
      }
    );
    if (!response.ok) throw new Error('Failed to update message');
    return response.json();
  },

  delete: async (teamId: number, messageId: number) => {
    const response = await fetch(
      `${API_BASE}/teams/${teamId}/chats/${messageId}`,
      {
        ...fetchOptions,
        method: 'DELETE',
      }
    );
    if (!response.ok) throw new Error('Failed to delete message');
    return true;
  },
};

const globalUsersApi = {
  registerOrLogin: async (request: LoginRequest): Promise<GlobalUser> => {
    const response = await fetch(`${API_BASE}/users/register-or-login`, {
      ...fetchOptions,
      method: 'POST',
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ 
        message: 'Failed to authenticate' 
      }));
      throw new Error(error.message || 'Failed to authenticate');
    }

    return response.json();
  },

  getById: async (id: number): Promise<GlobalUser> => {
    const response = await fetch(`${API_BASE}/users/${id}`, {
      ...fetchOptions,
      method: 'GET',
    });

    if (!response.ok) throw new Error('User not found');
    return response.json();
  },
};

const api = {
  teamsApi,
  usersApi,
  songsApi,
  ratingsApi,
  chatsApi,
  globalUsersApi,
  API_BASE,
};

export default api;
