import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import type { User } from './services/api.service';

import { NameEntry } from './components/NameEntry';
import { MainScreen } from './components/MainScreen';
import { CreateTeam } from './components/CreateTeam';
import { BrowseTeams } from './components/BrowseTeams';
import { JoinTeam } from './components/JoinTeam';
import { TeamView } from './views/TeamView';

// Define a type for user profiles
interface UserProfile {
  id: string;
  name: string;
  createdAt: string;
  lastUsed: string;
}

export default function App() {
  // Store multiple user profiles
  const [userProfiles, setUserProfiles] = useState<UserProfile[]>(() => {
    const savedProfiles = localStorage.getItem('userProfiles');
    return savedProfiles ? JSON.parse(savedProfiles) : [];
  });
  
  // Currently selected profile
  const [selectedProfileId, setSelectedProfileId] = useState<string>(() => {
    return sessionStorage.getItem('selectedProfileId') || '';
  });
  
  // Current active user in a team
  const [currentUser, setCurrentUser] = useState<User | null>(() => {
    const savedUser = sessionStorage.getItem('currentUser');
    return savedUser ? JSON.parse(savedUser) : null;
  });

  // Get the currently selected profile
  const selectedProfile = userProfiles.find(profile => profile.id === selectedProfileId);

  // Save user profiles to localStorage whenever they change
  useEffect(() => {
    localStorage.setItem('userProfiles', JSON.stringify(userProfiles));
  }, [userProfiles]);

  // Save selected profile ID to sessionStorage whenever it changes
  useEffect(() => {
    if (selectedProfileId) {
      sessionStorage.setItem('selectedProfileId', selectedProfileId);
    } else {
      sessionStorage.removeItem('selectedProfileId');
    }
  }, [selectedProfileId]);

  // Save currentUser to sessionStorage whenever it changes
  useEffect(() => {
    if (currentUser) {
      sessionStorage.setItem('currentUser', JSON.stringify(currentUser));
    } else {
      sessionStorage.removeItem('currentUser');
    }
  }, [currentUser]);

  // Add a new user profile
  const addUserProfile = (name: string) => {
    const newProfile: UserProfile = {
      id: Date.now().toString(),
      name,
      createdAt: new Date().toISOString(),
      lastUsed: new Date().toISOString()
    };
    
    setUserProfiles(prev => [...prev, newProfile]);
    setSelectedProfileId(newProfile.id);
  };

  // Select an existing profile
  const selectUserProfile = (id: string) => {
    setSelectedProfileId(id);
    
    // Update lastUsed timestamp
    setUserProfiles(prev => 
      prev.map(profile => 
        profile.id === id 
          ? { ...profile, lastUsed: new Date().toISOString() } 
          : profile
      )
    );
  };

  // Handle logout
  const handleLogout = () => {
    setSelectedProfileId('');
    setCurrentUser(null);
    sessionStorage.removeItem('selectedProfileId');
    sessionStorage.removeItem('currentUser');
  };

  // If no profile is selected, show the name entry screen
  if (!selectedProfile) {
    return <NameEntry 
      onSubmit={addUserProfile} 
      existingProfiles={userProfiles}
      onSelectProfile={selectUserProfile}
    />;
  }

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<MainScreen 
          onLogout={handleLogout} 
          profileName={selectedProfile.name}
        />} />
        <Route 
          path="/teams/create" 
          element={<CreateTeam userName={selectedProfile.name} onUserCreated={setCurrentUser} />} 
        />
        <Route 
          path="/teams/browse" 
          element={<BrowseTeams userName={selectedProfile.name} onUserCreated={setCurrentUser} />} 
        />
        <Route 
          path="/teams/join" 
          element={<JoinTeam userName={selectedProfile.name} onUserCreated={setCurrentUser} />} 
        />
        <Route 
          path="/teams/:teamId" 
          element={
            currentUser ? (
              <TeamView user={currentUser} onLeave={() => setCurrentUser(null)} />
            ) : (
              <Navigate to="/" replace />
            )
          } 
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
