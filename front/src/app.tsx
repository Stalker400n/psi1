import { BrowserRouter, Routes, Route, Navigate, useLocation, useNavigate, useParams } from 'react-router-dom';
import { useState, useEffect } from 'react';
import api from './services/api.service';
import type { User, GlobalUser } from './services/api.service';
import { useToast } from './contexts/toast-context';
import { ArrowLeft } from 'lucide-react';

import { NameEntry } from './components/name-entry.component';
import { MainPage } from './pages/main.page';
import { CreateTeamPage } from './pages/create.page';
import { BrowseTeamsPage } from './pages/browse.page';
import { JoinTeamPage } from './pages/join.page';
import { TeamPage } from './pages/team.page';
import { TeamJoinGate } from './components/join-gate.component';

interface TeamRouteHandlerProps {
  globalUser: GlobalUser | null;
  currentUser: User | null;
  isJoiningTeam: boolean;
  onLogin: (user: GlobalUser) => void;
  onLeave: () => void;
  onNavigateBack: () => void;
}

function TeamRouteHandler({ 
  globalUser, 
  currentUser, 
  isJoiningTeam, 
  onLogin,
  onLeave,
  onNavigateBack
}: TeamRouteHandlerProps) {
  const params = useParams<{ teamId: string }>();
  const teamId = params.teamId ? parseInt(params.teamId, 10) : null;
  
  if (!teamId || isNaN(teamId)) {
    return <Navigate to="/menu" replace />;
  }
  
  if (!globalUser) {
    return <TeamJoinGate 
      teamId={teamId} 
      onLogin={onLogin}
    />;
  }
  
  if (currentUser) {
    return <TeamPage user={currentUser} onLeave={onLeave} />;
  }
  
  if (isJoiningTeam) {
    return (
      <div className="h-screen w-screen flex items-center justify-center bg-slate-950 relative">
        <button 
          onClick={onNavigateBack}
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
    );
  }
  
  return <Navigate to="/menu" replace />;
}

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
  
  const [currentUser, setCurrentUser] = useState<User | null>(() => {
    const savedUser = sessionStorage.getItem('currentUser');
    return savedUser ? JSON.parse(savedUser) : null;
  });
  
  const [pendingTeamId, setPendingTeamId] = useState<number | null>(null);
  const [isJoiningTeam, setIsJoiningTeam] = useState<boolean>(false);
  
  useEffect(() => {
      if (location.pathname.startsWith('/teams/') && !currentUser) {
      const pathSegments = location.pathname.split('/');
      if (pathSegments.length >= 3) {
        const teamIdString = pathSegments[2];
        const teamId = parseInt(teamIdString, 10);
        if (!isNaN(teamId)) {
          console.log('Setting pending team ID from URL:', teamId);
          setPendingTeamId(teamId);
          return;
        }
      }
    }
    
    if (!location.pathname.startsWith('/teams/')) {
      console.log('Clearing pending team ID - not on team page');
      setPendingTeamId(null);
      setIsJoiningTeam(false);
    }
  }, [location.pathname, currentUser]);

  useEffect(() => {
    if (currentUser) {
      sessionStorage.setItem('currentUser', JSON.stringify(currentUser));
    } else {
      sessionStorage.removeItem('currentUser');
    }
  }, [currentUser]);

  useEffect(() => {
    if (currentUser && isJoiningTeam) {
      console.log('Current user set, clearing loading state');
      setIsJoiningTeam(false);
    }
  }, [currentUser, isJoiningTeam]);

  useEffect(() => {
    if (globalUser && pendingTeamId && !currentUser && !isJoiningTeam) {
      console.log('Auto-joining team for authenticated user');
      handleTeamJoin(globalUser, pendingTeamId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [globalUser, pendingTeamId, currentUser, isJoiningTeam]);

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

  const handleUserLogin = async (user: GlobalUser) => {
    console.log('User logged in:', user.name);
    setGlobalUser(user);
    sessionStorage.setItem('globalUserId', user.id.toString());
    sessionStorage.setItem('globalUserName', user.name);
    
    if (pendingTeamId) {
      await handleTeamJoin(user, pendingTeamId);
    }
  };

  const handleLogout = () => {
    setGlobalUser(null);
    setCurrentUser(null);
    sessionStorage.removeItem('globalUserId');
    sessionStorage.removeItem('globalUserName');
    sessionStorage.removeItem('currentUser');
  };
  
  useEffect(() => {
    if (!location.pathname.startsWith('/teams/')) {
      setIsJoiningTeam(false);
    }
  }, [location.pathname]);
  
  if (!globalUser) {
    return <NameEntry onSubmit={handleUserLogin} />;
  }

  return (
      <Routes>
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
        
        <Route 
          path="/menu" 
          element={
            globalUser ? (
              <MainPage 
                onLogout={handleLogout} 
                profileName={globalUser.name}
              />
            ) : (
              <Navigate to="/" replace />
            )
          } 
        />
        
        <Route 
          path="/create" 
          element={
            globalUser ? (
              <CreateTeamPage 
                userName={globalUser.name}
                userId={globalUser.id}
                onUserCreated={setCurrentUser} 
              />
            ) : (
              <Navigate to="/" replace />
            )
          } 
        />
        
        <Route 
          path="/teams" 
          element={
            globalUser ? (
              <BrowseTeamsPage 
                userName={globalUser.name}
                userId={globalUser.id}
                onUserCreated={setCurrentUser} 
              />
            ) : (
              <Navigate to="/" replace />
            )
          } 
        />
        
        <Route 
          path="/join" 
          element={
            globalUser ? (
              <JoinTeamPage 
                userName={globalUser.name}
                onUserCreated={setCurrentUser} 
              />
            ) : (
              <Navigate to="/" replace />
            )
          } 
        />
        
        <Route 
          path="/teams/:teamId" 
          element={<TeamRouteHandler 
            globalUser={globalUser}
            currentUser={currentUser}
            isJoiningTeam={isJoiningTeam}
            onLogin={handleUserLogin}
            onLeave={() => setCurrentUser(null)}
            onNavigateBack={() => {
              setIsJoiningTeam(false);
              setPendingTeamId(null);
              navigate('/menu');
            }}
          />}
        />
        
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
  );
}
