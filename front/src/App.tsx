import {useState} from 'react';
import type {User, Team} from './services/api.service';

// Import components
import {NameEntry} from './components/NameEntry';
import {MainScreen} from './components/MainScreen';
import {CreateTeam} from './components/CreateTeam';
import {BrowseTeams} from './components/BrowseTeams';
import {JoinTeam} from './components/JoinTeam';
import {TeamView} from './views/TeamView';

type ViewType = 'home' | 'create' | 'browse' | 'join' | 'team';

export default function App() {
  const [view, setView] = useState<ViewType>('home');
  const [userName, setUserName] = useState<string>('');
  const [currentTeam, setCurrentTeam] = useState<Team | null>(null);
  const [currentUser, setCurrentUser] = useState<User | null>(null);

  const handleViewChange = (newView: ViewType) => {
    console.log('Changing view to:', newView);
    setView(newView);
  };

  const handleTeamCreated = (team: Team, user: User) => {
    console.log('Team created:', team);
    setCurrentTeam(team);
    setCurrentUser(user);
    setView('team');
  };

  const handleTeamJoined = (team: Team, user: User) => {
    console.log('Team joined:', team);
    setCurrentTeam(team);
    setCurrentUser(user);
    setView('team');
  };

  const handleLeaveTeam = () => {
    console.log('Leaving team');
    setCurrentTeam(null);
    setCurrentUser(null);
    setView('home');
  };

  if (!userName) {
    return <NameEntry onSubmit={setUserName} />;
  }

  if (!currentTeam && view === 'home') {
    return (
      <MainScreen
        onCreateTeam={() => handleViewChange('create')}
        onBrowseTeams={() => handleViewChange('browse')}
        onJoinTeam={() => handleViewChange('join')}
      />
    );
  }

  if (view === 'create') {
    return (
      <CreateTeam
        userName={userName}
        onBack={() => handleViewChange('home')}
        onTeamCreated={handleTeamCreated}
      />
    );
  }

  if (view === 'browse') {
    return (
      <BrowseTeams
        onBack={() => handleViewChange('home')}
        onJoinTeam={handleTeamJoined}
        userName={userName}
      />
    );
  }

  if (view === 'join') {
    return (
      <JoinTeam
        userName={userName}
        onBack={() => handleViewChange('home')}
        onTeamJoined={handleTeamJoined}
      />
    );
  }

  if (currentTeam && currentUser) {
    return (
      <TeamView
        team={currentTeam}
        user={currentUser}
        onLeave={handleLeaveTeam}
      />
    );
  }

  return (
    <MainScreen
      onCreateTeam={() => handleViewChange('create')}
      onBrowseTeams={() => handleViewChange('browse')}
      onJoinTeam={() => handleViewChange('join')}
    />
  );
}
