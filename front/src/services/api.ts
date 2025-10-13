// API service for interacting with the backend

// Types matching backend models
export interface User {
  id: number;
  name: string;
  score: number;
  joinedAt: string;
  isActive: boolean;
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
  songs: Song[];
  users: User[];
  messages: ChatMessage[];
}

// Use HTTPS as requested
const API_BASE = 'https://localhost:7130';

// Common fetch options to handle HTTPS certificate issues
const fetchOptions = {
  headers: { 'Content-Type': 'application/json' },
  mode: 'cors' as RequestMode,
  // For development only - don't validate SSL certificates
  credentials: 'omit' as RequestCredentials
};

// Team related API calls
const teamsApi = {
  // Get all teams
  getAll: async (): Promise<Team[]> => {
    const response = await fetch(`${API_BASE}/teams`, { 
      ...fetchOptions,
      method: 'GET'
    });
    if (!response.ok) throw new Error('Failed to fetch teams');
    return response.json();
  },

  // Get a team by ID
  getById: async (id: number): Promise<Team> => {
    const response = await fetch(`${API_BASE}/teams/${id}`, { 
      ...fetchOptions,
      method: 'GET'
    });
    if (!response.ok) throw new Error('Team not found');
    return response.json();
  },

  // Create a new team
  create: async (team: { name: string; isPrivate: boolean }): Promise<Team> => {
    const response = await fetch(`${API_BASE}/teams`, {
      ...fetchOptions,
      method: 'POST',
      body: JSON.stringify(team)
    });
    if (!response.ok) throw new Error('Failed to create team');
    return response.json();
  },

  // Update a team
  update: async (id: number, team: { name: string; isPrivate: boolean }): Promise<Team> => {
    const response = await fetch(`${API_BASE}/teams/${id}`, {
      ...fetchOptions,
      method: 'PUT',
      body: JSON.stringify(team)
    });
    if (!response.ok) throw new Error('Failed to update team');
    return response.json();
  },

  // Delete a team
  delete: async (id: number) => {
    const response = await fetch(`${API_BASE}/teams/${id}`, {
      ...fetchOptions,
      method: 'DELETE'
    });
    if (!response.ok) throw new Error('Failed to delete team');
    return true;
  }
};

// User related API calls
const usersApi = {
  // Get all users in a team
  getAll: async (teamId: number): Promise<User[]> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/users`, {
      ...fetchOptions,
      method: 'GET'
    });
    if (!response.ok) throw new Error('Failed to fetch users');
    return response.json();
  },

  // Get a user by ID
  getById: async (teamId: number, userId: number): Promise<User> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/users/${userId}`, {
      ...fetchOptions,
      method: 'GET'
    });
    if (!response.ok) throw new Error('User not found');
    return response.json();
  },

  // Add a user to a team
  add: async (teamId: number, user: { name: string; score: number; isActive: boolean }): Promise<User> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/users`, {
      ...fetchOptions,
      method: 'POST',
      body: JSON.stringify(user)
    });
    if (!response.ok) throw new Error('Failed to add user');
    return response.json();
  },

  // Update a user
  update: async (teamId: number, userId: number, user: { name: string; score: number; isActive: boolean }): Promise<User> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/users/${userId}`, {
      ...fetchOptions,
      method: 'PUT',
      body: JSON.stringify(user)
    });
    if (!response.ok) throw new Error('Failed to update user');
    return response.json();
  },

  // Delete a user
  delete: async (teamId: number, userId: number) => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/users/${userId}`, {
      ...fetchOptions,
      method: 'DELETE'
    });
    if (!response.ok) throw new Error('Failed to delete user');
    return true;
  }
};

// Song related API calls
const songsApi = {
  // Get all songs in a team
  getAll: async (teamId: number): Promise<Song[]> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs`, {
      ...fetchOptions,
      method: 'GET'
    });
    if (!response.ok) throw new Error('Failed to fetch songs');
    return response.json();
  },

  // Get a song by ID
  getById: async (teamId: number, songId: number): Promise<Song> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs/${songId}`, {
      ...fetchOptions,
      method: 'GET'
    });
    if (!response.ok) throw new Error('Song not found');
    return response.json();
  },

  // Add a song to a team
  add: async (teamId: number, song: { 
    link: string; 
    title: string; 
    artist: string; 
    rating: number;
    addedByUserId: number;
    addedByUserName: string;
  }): Promise<Song> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs`, {
      ...fetchOptions,
      method: 'POST',
      body: JSON.stringify(song)
    });
    if (!response.ok) throw new Error('Failed to add song');
    return response.json();
  },

  // Update a song
  update: async (teamId: number, songId: number, song: { 
    link: string; 
    title: string; 
    artist: string; 
    rating: number;
  }): Promise<Song> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs/${songId}`, {
      ...fetchOptions,
      method: 'PUT',
      body: JSON.stringify(song)
    });
    if (!response.ok) throw new Error('Failed to update song');
    return response.json();
  },

  // Delete a song
  delete: async (teamId: number, songId: number) => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/songs/${songId}`, {
      ...fetchOptions,
      method: 'DELETE'
    });
    if (!response.ok) throw new Error('Failed to delete song');
    return true;
  }
};

// Chat related API calls
const chatsApi = {
  // Get all messages in a team
  getAll: async (teamId: number): Promise<ChatMessage[]> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/chats`, {
      ...fetchOptions,
      method: 'GET'
    });
    if (!response.ok) throw new Error('Failed to fetch messages');
    return response.json();
  },

  // Get a message by ID
  getById: async (teamId: number, messageId: number): Promise<ChatMessage> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/chats/${messageId}`, {
      ...fetchOptions,
      method: 'GET'
    });
    if (!response.ok) throw new Error('Message not found');
    return response.json();
  },

  // Add a message to a team
  add: async (teamId: number, message: { userName: string; text: string }): Promise<ChatMessage> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/chats`, {
      ...fetchOptions,
      method: 'POST',
      body: JSON.stringify(message)
    });
    if (!response.ok) throw new Error('Failed to add message');
    return response.json();
  },

  // Update a message
  update: async (teamId: number, messageId: number, message: { text: string }): Promise<ChatMessage> => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/chats/${messageId}`, {
      ...fetchOptions,
      method: 'PUT',
      body: JSON.stringify(message)
    });
    if (!response.ok) throw new Error('Failed to update message');
    return response.json();
  },

  // Delete a message
  delete: async (teamId: number, messageId: number) => {
    const response = await fetch(`${API_BASE}/teams/${teamId}/chats/${messageId}`, {
      ...fetchOptions,
      method: 'DELETE'
    });
    if (!response.ok) throw new Error('Failed to delete message');
    return true;
  }
};

const api = {
  teamsApi,
  usersApi,
  songsApi,
  chatsApi,
  API_BASE
};

export default api;
