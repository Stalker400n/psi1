import { BrowserRouter, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { useState, useEffect } from 'react';
import api from './services/api.service';
import type { User, GlobalUser } from './services/api.service';
import { useToast } from './contexts/ToastContext';

import { NameEntry } from './components/NameEntry';
import { MainScreen } from './components/MainScreen';
import { CreateTeam } from './components/CreateTeam';
import { BrowseTeams } from './components/BrowseTeams';
import { JoinTeam } from './components/JoinTeam';
import { TeamView } from './views/TeamView';
import { TeamJoinGate } from './components/TeamJoinGate';
// No longer needed: import { TeamIdCapture } from './components/TeamIdCapture';

// App.tsx - Main application component

export default function App() {
  return (
    <BrowserRouter>
      <AppContent />
    </BrowserRouter>
  );
}

function AppContent() {
  const { showToast } = useToast();
  const location = useLocation();
  
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
  
  // Track pending team join
  const [pendingTeamId, setPendingTeamId] = useState<number | null>(null);
  
  // Extract team ID from URL when component mounts or URL changes
  useEffect(() => {
    // Check if we're on a team page and not already in a team
    if (location.pathname.startsWith('/teams/') && !currentUser) {
      const pathSegments = location.pathname.split('/');
      if (pathSegments.length >= 3) {
        const teamIdString = pathSegments[2];
        const teamId = parseInt(teamIdString, 10);
        if (!isNaN(teamId)) {
          console.log('Setting pending team ID from URL:', teamId);
          setPendingTeamId(teamId);
        }
      }
    }
  }, [location.pathname, currentUser]);

  // Save currentUser to sessionStorage whenever it changes
  useEffect(() => {
    if (currentUser) {
      sessionStorage.setItem('currentUser', JSON.stringify(currentUser));
    } else {
      sessionStorage.removeItem('currentUser');
    }
  }, [currentUser]);

  // Handle user login with fingerprinting
  const handleUserLogin = async (user: GlobalUser) => {
    console.log('User logged in:', user.name);
    setGlobalUser(user);
    sessionStorage.setItem('globalUserId', user.id.toString());
    sessionStorage.setItem('globalUserName', user.name);
    
    // If there's a pending team join, handle it
    if (pendingTeamId) {
      console.log('Handling pending team join for team ID:', pendingTeamId);
      try {
        // Get team to check if user already in it
        const team = await api.teamsApi.getById(pendingTeamId);
        console.log('Team fetched successfully:', team.name);
        
        const existingUser = team.users?.find(u => u.name === user.name);
        
        if (existingUser) {
          // User already in team
          console.log('User already in team, setting as current user');
          setCurrentUser(existingUser);
          showToast(`Welcome back to ${team.name}!`, 'success');
        } else {
          // Join the team
          console.log('User not in team, adding to team');
          const teamUser = await api.usersApi.add(pendingTeamId, {
            name: user.name,
            score: 0,
            isActive: true
          });
          console.log('User added to team successfully:', teamUser);
          setCurrentUser(teamUser);
          showToast(`You've joined ${team.name}!`, 'success');
        }
      } catch (error) {
        console.error('Failed to join team:', error);
        showToast('Failed to join team. Please try again.', 'error');
      }
    }
  };

  // Handle logout
  const handleLogout = () => {
    setGlobalUser(null);
    setCurrentUser(null);
    sessionStorage.removeItem('globalUserId');
    sessionStorage.removeItem('globalUserName');
    sessionStorage.removeItem('currentUser');
  };

  // Effect to handle team join navigation - directly navigate to team page
  useEffect(() => {
    // If user is logged in, has a team ID but not yet in a team
    if (globalUser && pendingTeamId && !currentUser) {
      console.log('Processing team join after authentication');
      // The handleUserLogin function will have already tried to join the team
      // We don't need a redirect here since the routes will handle it
    }
  }, [globalUser, pendingTeamId, currentUser]);
  
  // If no global user AND we have a pending team join, show special gate
  if (!globalUser && pendingTeamId) {
    return (
      <TeamJoinGate 
        teamId={pendingTeamId}
        onLogin={handleUserLogin}
      />
    );
  }

  // If no global user, show the name entry screen
  if (!globalUser) {
    return <NameEntry onSubmit={handleUserLogin} />;
  }

  return (
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
            // If we have a current user (already in the team), show TeamView
            currentUser ? (
              <TeamView user={currentUser} onLeave={() => setCurrentUser(null)} />
            ) : globalUser && pendingTeamId ? (
              // If user is authenticated and we have a pending team join,
              // stay on this page which will trigger team join in useEffect
              <div className="h-screen w-screen flex items-center justify-center bg-slate-950">
                <div className="text-white text-center">
                  <div className="animate-pulse mb-3">Joining team...</div>
                  <div className="text-sm text-slate-400">Please wait</div>
                </div>
              </div>
            ) : (
              // Otherwise redirect to main menu
              <Navigate to="/" replace />
            )
          } 
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
  );
}
