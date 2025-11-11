import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import type { User } from './services/api.service';

import { NameEntry } from './components/NameEntry';
import { MainScreen } from './components/MainScreen';
import { CreateTeam } from './components/CreateTeam';
import { BrowseTeams } from './components/BrowseTeams';
import { JoinTeam } from './components/JoinTeam';
import { TeamView } from './views/TeamView';

export default function App() {
  const [userName, setUserName] = useState<string>(() => {
    return localStorage.getItem('userName') || '';
  });
  
  const [currentUser, setCurrentUser] = useState<User | null>(() => {
    const savedUser = localStorage.getItem('currentUser');
    return savedUser ? JSON.parse(savedUser) : null;
  });

  useEffect(() => {
    if (userName) {
      localStorage.setItem('userName', userName);
    } else {
      localStorage.removeItem('userName');
    }
  }, [userName]);

  useEffect(() => {
    if (currentUser) {
      localStorage.setItem('currentUser', JSON.stringify(currentUser));
    } else {
      localStorage.removeItem('currentUser');
    }
  }, [currentUser]);

  const handleLogout = () => {
    setUserName('');
    setCurrentUser(null);
  };

  if (!userName) {
    return <NameEntry onSubmit={setUserName} />;
  }

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<MainScreen onLogout={handleLogout} />} />
        <Route 
          path="/teams/create" 
          element={<CreateTeam userName={userName} onUserCreated={setCurrentUser} />} 
        />
        <Route 
          path="/teams/browse" 
          element={<BrowseTeams userName={userName} onUserCreated={setCurrentUser} />} 
        />
        <Route 
          path="/teams/join" 
          element={<JoinTeam userName={userName} onUserCreated={setCurrentUser} />} 
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
