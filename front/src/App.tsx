import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import type { User, GlobalUser } from './services/api.service';

import { NameEntry } from './components/NameEntry';
import { MainScreen } from './components/MainScreen';
import { CreateTeam } from './components/CreateTeam';
import { BrowseTeams } from './components/BrowseTeams';
import { JoinTeam } from './components/JoinTeam';
import { TeamView } from './views/TeamView';

// App.tsx - Main application component

export default function App() {
  // Global user data (from fingerprint authentication)
  const [globalUser, setGlobalUser] = useState<GlobalUser | null>(() => {
    const savedId = sessionStorage.getItem('globalUserId');
    const savedName = sessionStorage.getItem('globalUserName');
    
    if (savedId && savedName) {
      return { 
        id: parseInt(savedId), 
        name: savedName, 
        isNew: false 
      };
    }
    return null;
  });
  
  // Current active user in a team
  const [currentUser, setCurrentUser] = useState<User | null>(() => {
    const savedUser = sessionStorage.getItem('currentUser');
    return savedUser ? JSON.parse(savedUser) : null;
  });

  // Save currentUser to sessionStorage whenever it changes
  useEffect(() => {
    if (currentUser) {
      sessionStorage.setItem('currentUser', JSON.stringify(currentUser));
    } else {
      sessionStorage.removeItem('currentUser');
    }
  }, [currentUser]);

  // Handle user login with fingerprinting
  const handleUserLogin = (user: GlobalUser) => {
    setGlobalUser(user);
    sessionStorage.setItem('globalUserId', user.id.toString());
    sessionStorage.setItem('globalUserName', user.name);
  };

  // Handle logout
  const handleLogout = () => {
    setGlobalUser(null);
    setCurrentUser(null);
    sessionStorage.removeItem('globalUserId');
    sessionStorage.removeItem('globalUserName');
    sessionStorage.removeItem('currentUser');
  };

  // If no global user, show the name entry screen
  if (!globalUser) {
    return <NameEntry onSubmit={handleUserLogin} />;
  }

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<MainScreen 
          onLogout={handleLogout} 
          profileName={globalUser.name}
        />} />
        <Route 
          path="/teams/create" 
          element={<CreateTeam 
            userName={globalUser.name} 
            userId={globalUser.id}
            onUserCreated={setCurrentUser} 
          />} 
        />
        <Route 
          path="/teams/browse" 
          element={<BrowseTeams userName={globalUser.name} onUserCreated={setCurrentUser} />} 
        />
        <Route 
          path="/teams/join" 
          element={<JoinTeam userName={globalUser.name} onUserCreated={setCurrentUser} />} 
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
