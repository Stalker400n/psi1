import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useState } from 'react';
import type { User } from './services/api.service';

import { NameEntry } from './components/NameEntry';
import { MainScreen } from './components/MainScreen';
import { CreateTeam } from './components/CreateTeam';
import { BrowseTeams } from './components/BrowseTeams';
import { JoinTeam } from './components/JoinTeam';
import { TeamView } from './views/TeamView';

export default function App() {
  const [userName, setUserName] = useState<string>('');
  const [currentUser, setCurrentUser] = useState<User | null>(null);

  if (!userName) {
    return <NameEntry onSubmit={setUserName} />;
  }

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<MainScreen />} />
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