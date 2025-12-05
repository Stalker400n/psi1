import { BrowserRouter, Routes, Route, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import api from './services/api.service';
import type { User, GlobalUser } from './services/api.service';
import { useToast } from './contexts/ToastContext';
import { ArrowLeft } from 'lucide-react';

import { NameEntry } from './components/NameEntry';
import { MainScreen } from './components/MainScreen';
import { CreateTeam } from './components/CreateTeam';
import { BrowseTeams } from './components/BrowseTeams';
import { JoinTeam } from './components/JoinTeam';
import { TeamView } from './views/TeamView';
import { TeamJoinGate } from './components/TeamJoinGate';

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
  const navigate = useNavigate();
  
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
  
  // Track team join loading state
  const [isJoiningTeam, setIsJoiningTeam] = useState<boolean>(false);
  
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
          return; // Early return - we're done
        }
      }
    }
    
    // If we're NOT on a team page, clear pending team ID and loading state
    if (!location.pathname.startsWith('/teams/')) {
      console.log('Clearing pending team ID - not on team page');
      setPendingTeamId(null);
      setIsJoiningTeam(false);  // Also clear loading state
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

  // Clear loading state when currentUser is set (team join completed)
  useEffect(() => {
    if (currentUser && isJoiningTeam) {
      console.log('Current user set, clearing loading state');
      setIsJoiningTeam(false);
    }
  }, [currentUser, isJoiningTeam]);

  // Auto-join team when authenticated user navigates to team URL
  useEffect(() => {
    // Only run if:
    // 1. User is authenticated (globalUser exists)
    // 2. We have a pending team ID
    // 3. User is not already in a team (currentUser is null)
    // 4. Not already processing a join (isJoiningTeam is false)
    if (globalUser && pendingTeamId && !currentUser && !isJoiningTeam) {
      console.log('Auto-joining team for authenticated user');
      handleTeamJoin(globalUser, pendingTeamId);
    }
  }, [globalUser, pendingTeamId, currentUser, isJoiningTeam]);

  // Handle team join logic (extracted from handleUserLogin)
  const handleTeamJoin = async (user: GlobalUser, teamId: number) => {
    setIsJoiningTeam(true);
    console.log('Joining team ID:', teamId);
    try {
      const team = await api.teamsApi.getById(teamId);
      console.log('Team fetched successfully:', team.name);
      
      const existingUser = team.users?.find(u => u.name === user.name);
      
      if (existingUser) {
        console.log('User already in team, setting as current user');
        setCurrentUser(existingUser);
      } else {
        console.log('User not in team, adding to team');
        const teamUser = await api.usersApi.add(teamId, {
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
      setPendingTeamId(null);
      setIsJoiningTeam(false);
    }
  };

  // Handle user login with fingerprinting
  const handleUserLogin = async (user: GlobalUser) => {
    console.log('User logged in:', user.name);
    setGlobalUser(user);
    sessionStorage.setItem('globalUserId', user.id.toString());
    sessionStorage.setItem('globalUserName', user.name);
    
    // If there's a pending team join, handle it
    if (pendingTeamId) {
      await handleTeamJoin(user, pendingTeamId);
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
  
  // Clear loading state when navigating away from team route
  useEffect(() => {
    if (!location.pathname.startsWith('/teams/')) {
      setIsJoiningTeam(false);
    }
  }, [location.pathname]);
  
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
        {/* Login screen - always at root */}
        <Route 
          path="/" 
          element={
            globalUser ? (
              <Navigate to="/menu" replace />
            ) : (
              <NameEntry onSubmit={handleUserLogin} />
            )
          } 
        />
        
        {/* Main menu - requires login */}
        <Route 
          path="/menu" 
          element={
            globalUser ? (
              <MainScreen 
                onLogout={handleLogout} 
                profileName={globalUser.name}
              />
            ) : (
              <Navigate to="/" replace />
            )
          } 
        />
        
        {/* Create team */}
        <Route 
          path="/create" 
          element={
            globalUser ? (
              <CreateTeam 
                userName={globalUser.name}
                userId={globalUser.id}
                onUserCreated={setCurrentUser} 
              />
            ) : (
              <Navigate to="/" replace />
            )
          } 
        />
        
        {/* Browse teams */}
        <Route 
          path="/teams" 
          element={
            globalUser ? (
              <BrowseTeams 
                userName={globalUser.name}
                userId={globalUser.id}
                onUserCreated={setCurrentUser} 
              />
            ) : (
              <Navigate to="/" replace />
            )
          } 
        />
        
        {/* Join with code */}
        <Route 
          path="/join" 
          element={
            globalUser ? (
              <JoinTeam 
                userName={globalUser.name}
                onUserCreated={setCurrentUser} 
              />
            ) : (
              <Navigate to="/" replace />
            )
          } 
        />
        
        {/* Team view */}
        <Route 
          path="/teams/:teamId" 
          element={
            // If no global user but has a team ID, show the special gate
            !globalUser ? (
              <TeamJoinGate 
                teamId={pendingTeamId!}
                onLogin={handleUserLogin}
              />
            ) : currentUser ? (
              // If we have a current user (already in the team), show TeamView
              <TeamView user={currentUser} onLeave={() => setCurrentUser(null)} />
            ) : isJoiningTeam ? (
              // Currently processing team join - show loading with back button
              <div className="h-screen w-screen flex items-center justify-center bg-slate-950 relative">
                <button 
                  onClick={() => {
                    setIsJoiningTeam(false);
                    setPendingTeamId(null);
                    navigate('/menu');
                  }}
                  className="absolute top-8 right-8 text-slate-400 hover:text-white flex items-center gap-2 transition"
                >
                  <ArrowLeft size={20} />
                  Back
                </button>
                <div className="text-white text-center">
                  <div className="animate-pulse mb-3">Joining team...</div>
                  <div className="text-sm text-slate-400">Please wait</div>
                </div>
              </div>
            ) : (
              // Otherwise redirect to main menu
              <Navigate to="/menu" replace />
            )
          } 
        />
        
        {/* Catch-all redirect */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
  );
}