import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Users, Lock, Crown, Shield, Filter, ArrowUpDown, User as UserIcon, UserPlus } from 'lucide-react';
import api from '../services/api.service';
import type { Team, User } from '../services/api.service';
import { renderPulsingStar, floatingQuotesCSS } from '../utils/praise.util';
import { useToast } from '../contexts/toast-context';

interface BrowseTeamsPageProps {
  userName: string;
  userId: number;
  onUserCreated: (user: User) => void;
}

type SortType = 'most-members' | 'least-members' | 'alpha' | 'by-role' | 'newest' | 'oldest';

interface FiltersState {
  public: boolean;
  private: boolean;
  joined: boolean;
  owner: boolean;
  moderator: boolean;
}

export function BrowseTeamsPage({ userName, userId, onUserCreated }: BrowseTeamsPageProps) {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [teams, setTeams] = useState<Team[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  
  const [filters, setFilters] = useState<FiltersState>({
    public: true,
    private: true,
    joined: true,
    owner: true,
    moderator: true,
  });
  
  const [sort, setSort] = useState<SortType>('most-members');

  useEffect(() => {
    fetchTeams();
    
    console.log(`User ${userName} (ID: ${userId}) browsing teams`);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userName, userId]);

  const fetchTeams = async () => {
    try {
      const data = await api.teamsApi.getAll();
      setTeams(data);
    } catch (error) {
      console.error('Error fetching teams:', error);
      showToast('Failed to load teams', 'error');
    }
    setLoading(false);
  };

  const toggleFilter = (filterName: keyof FiltersState) => {
    setFilters(prevFilters => ({
      ...prevFilters,
      [filterName]: !prevFilters[filterName]
    }));
  };

  const handleJoin = async (team: Team) => {
    try {
      const existingUser = team.users?.find(u => u.name === userName);
      
      if (existingUser) {
        onUserCreated(existingUser);
        navigate(`/teams/${team.id}`);
        return;
      }
      
      const user = await api.usersApi.add(team.id, { 
        name: userName, 
        score: 0, 
        isActive: true
      });
      
      console.log(`User ${userName} (ID: ${userId}) joined team ${team.name} (ID: ${team.id})`);
      onUserCreated(user);
      navigate(`/teams/${team.id}`);
    } catch (error) {
      console.error('Error joining team:', error);
      showToast('Failed to join team', 'error');
    }
  };

  const getUserRole = (team: Team): 'owner' | 'moderator' | 'member' | null => {
    const user = team.users?.find(u => u.name === userName);
    if (!user) return null;
    
    if (user.role === 'Owner') return 'owner';
    if (user.role === 'Moderator') return 'moderator';
    return 'member';
  };

  const isUserInTeam = (team: Team): boolean => {
    return team.users?.some(u => u.name === userName) || false;
  };
  
  const getRolePriority = (team: Team): number => {
    const role = getUserRole(team);
    
    if (role === 'owner') return 1;
    if (role === 'moderator') return 2;
    if (role === 'member') return 3;
    return 4;
  };

  const getFilteredTeams = (): Team[] => {
    if (!Object.values(filters).some(value => value)) {
      return [];
    }

    return teams.filter(team => {
      if (filters.public && !team.isPrivate) {
        return true;
      }
      
      if (filters.private && team.isPrivate && isUserInTeam(team)) {
        return true;
      }
      
      if (filters.joined && isUserInTeam(team)) {
        return true;
      }
      
      if (filters.owner && getUserRole(team) === 'owner') {
        return true;
      }
      
      if (filters.moderator && getUserRole(team) === 'moderator') {
        return true;
      }
      
      return false;
    });
  };

  const sortTeams = (teams: Team[]): Team[] => {
    const teamsToSort = [...teams];
    
    const splitByRoleFirst = sort === 'by-role';
    
    if (splitByRoleFirst) {
      return teamsToSort.sort((a, b) => {
        const roleA = getRolePriority(a);
        const roleB = getRolePriority(b);
        if (roleA !== roleB) {
          return roleA - roleB;
        }
        
        const nameComparison = a.name.localeCompare(b.name);
        if (nameComparison !== 0) {
          return nameComparison;
        }
        
        const dateComparison = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        if (dateComparison !== 0) {
          return dateComparison;
        }
        
        return (b.users?.length || 0) - (a.users?.length || 0);
      });
    }
    
    return teamsToSort.sort((a, b) => {
      let result = 0;
      
      switch (sort) {
        case 'alpha':
          result = a.name.localeCompare(b.name);
          break;
        case 'newest':
          result = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
          break;
        case 'oldest':
          result = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
          break;
        case 'most-members':
          result = (b.users?.length || 0) - (a.users?.length || 0);
          break;
        case 'least-members':
          result = (a.users?.length || 0) - (b.users?.length || 0);
          break;
      }
      
      if (result !== 0) {
        return result;
      }
      
      const roleA = getRolePriority(a);
      const roleB = getRolePriority(b);
      if (roleA !== roleB) {
        return roleA - roleB;
      }
      
      if (sort !== 'alpha') {
        const nameComparison = a.name.localeCompare(b.name);
        if (nameComparison !== 0) {
          return nameComparison;
        }
      }
      
      if (sort !== 'newest' && sort !== 'oldest') {
        const dateComparison = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        if (dateComparison !== 0) {
          return dateComparison;
        }
      }
      
      if (sort !== 'most-members' && sort !== 'least-members') {
        const memberComparison = (b.users?.length || 0) - (a.users?.length || 0);
        if (memberComparison !== 0) {
          return memberComparison;
        }
      }
      
      return a.id - b.id;
    });
  };
  
  const getSortedTeams = (filtered: Team[]): Team[] => {
    return sortTeams(filtered);
  };

  const displayTeams = getSortedTeams(getFilteredTeams());

  const getTeamButtonStyle = (team: Team): string => {
    if (!isUserInTeam(team)) {
      return 'bg-yellow-500 text-black hover:bg-yellow-400';
    }

    const role = getUserRole(team);
    if (role === 'owner') {
      return 'bg-green-600 text-white hover:bg-green-500';
    }
    if (role === 'moderator') {
      return 'bg-magenta-600 text-white hover:bg-magenta-500';
    }
    return 'bg-blue-600 text-white hover:bg-blue-500';
  };

  const getButtonText = (): string => {
    return 'Connect';
  };

  const ToggleSwitch = ({ 
    label, 
    checked, 
    onChange 
  }: { 
    label: string; 
    checked: boolean; 
    onChange: () => void;
  }) => (
    <label className="flex items-center justify-between cursor-pointer group">
      <span className="text-slate-300 group-hover:text-slate-200">{label}</span>
      <div className="relative">
        <input
          type="checkbox"
          checked={checked}
          onChange={onChange}
          className="sr-only"
        />
        <div className={`block w-10 h-6 rounded-full transition-colors ${
          checked ? 'bg-yellow-500' : 'bg-slate-700'
        }`} />
        <div className={`absolute left-1 top-1 bg-white w-4 h-4 rounded-full transition-transform ${
          checked ? 'transform translate-x-4' : ''
        }`} />
      </div>
    </label>
  );

  return (
    <div className="min-h-screen bg-slate-950 p-8">
      <button 
        onClick={() => navigate('/menu')} 
        className="text-slate-400 hover:text-white mb-8 flex items-center gap-2 transition"
      >
        <ArrowLeft size={20} />
        Back
      </button>
      
      <h1 className="text-5xl font-bold text-white -mt-7 mb-7 text-center">
        Browse Teams{renderPulsingStar({ className: 'text-yellow-400' })}
      </h1>
      
      <div className="max-w-6xl mx-auto mb-6">
        <div className="flex flex-col md:flex-row gap-4">
          <div className="bg-slate-900 p-4 rounded-lg border border-slate-800 md:w-64">
            <div className="flex items-center gap-2 mb-3">
              <Filter size={18} className="text-yellow-500" />
              <h3 className="text-white font-semibold">Filter by</h3>
            </div>
            <div className="flex flex-col gap-3">
              <ToggleSwitch 
                label="Public" 
                checked={filters.public} 
                onChange={() => toggleFilter('public')} 
              />
              <ToggleSwitch 
                label="Private" 
                checked={filters.private} 
                onChange={() => toggleFilter('private')} 
              />
              <ToggleSwitch 
                label="Joined" 
                checked={filters.joined} 
                onChange={() => toggleFilter('joined')} 
              />
              <ToggleSwitch 
                label="Owner" 
                checked={filters.owner} 
                onChange={() => toggleFilter('owner')} 
              />
              <ToggleSwitch 
                label="Moderator" 
                checked={filters.moderator} 
                onChange={() => toggleFilter('moderator')} 
              />
            </div>
          </div>

          <div className="bg-slate-900 p-4 rounded-lg border border-slate-800 flex-1">
            <div className="flex items-center gap-2 mb-3">
              <ArrowUpDown size={18} className="text-yellow-500" />
              <h3 className="text-white font-semibold">Sort by</h3>
            </div>
            <div className="grid grid-cols-2 gap-2">
              {[
                ['most-members', 'Most Members'],
                ['least-members', 'Least Members'],
                ['alpha', 'A-Z'],
                ['by-role', 'By Role'],
                ['newest', 'Newest'],
                ['oldest', 'Oldest']
              ].map(([value, label]) => (
                <button
                  key={value}
                  onClick={() => setSort(value as SortType)}
                  className={`px-3 py-2 rounded text-sm font-medium transition ${
                    sort === value
                      ? 'bg-yellow-500 text-black'
                      : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                  }`}
                >
                  {label}
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>

      <p className="text-slate-400 text-center mb-8">
        {loading ? 'Loading...' : `${displayTeams.length} ${displayTeams.length === 1 ? 'team' : 'teams'}`}
      </p>
      
      <div className="max-w-6xl mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {loading ? (
            Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="bg-slate-800/50 p-6 rounded-lg animate-pulse">
                <div className="h-8 bg-slate-700 rounded mb-3"></div>
                <div className="h-4 bg-slate-700 rounded w-2/3 mb-2"></div>
                <div className="h-4 bg-slate-700 rounded w-1/2 mb-4"></div>
                <div className="h-10 bg-slate-700 rounded"></div>
              </div>
            ))
          ) : displayTeams.length === 0 ? (
            <div className="col-span-full text-center py-12">
              <Users size={48} className="text-slate-700 mx-auto mb-4" />
              <p className="text-slate-400">No teams found matching your filters</p>
            </div>
          ) : (
            displayTeams.map(team => {
              const role = getUserRole(team);
              const inTeam = isUserInTeam(team);
              
              return (
                <div 
                  key={team.id} 
                  className={`bg-slate-800 p-6 rounded-lg hover:bg-slate-750 transition flex flex-col border ${
                    role === 'owner' ? 'border-green-500/50' :
                    role === 'moderator' ? 'border-magenta-500/50' :
                    inTeam ? 'border-blue-500/50' :
                    'border-slate-700 hover:border-yellow-500/50'
                  }`}
                >
                  <div className="flex-1 mb-4">
                    <div className="flex items-start justify-between mb-2">
                      <h3 className="text-2xl text-white font-bold">{team.name}</h3>
                      {team.isPrivate && (
                        <Lock size={20} className="text-slate-400" />
                      )}
                    </div>
                    
                    <p className="text-slate-400 text-sm mb-1">Code: {team.id}</p>
                    
                    <div className="flex items-center gap-2 text-slate-500 text-xs mb-2">
                      <Users size={14} />
                      <span>{team.users?.length || 0} {team.users?.length === 1 ? 'member' : 'members'}</span>
                    </div>

                    <div className="flex items-center gap-1 mt-2">
                      {role === 'owner' && (
                        <>
                          <Crown size={14} className="text-green-500" />
                          <span className="text-green-500 text-xs font-semibold">Owner</span>
                        </>
                      )}
                      {role === 'moderator' && (
                        <>
                          <Shield size={14} className="text-magenta-500" />
                          <span className="text-magenta-500 text-xs font-semibold">Moderator</span>
                        </>
                      )}
                      {role === 'member' && (
                        <>
                          <UserIcon size={14} className="text-blue-500" />
                          <span className="text-blue-500 text-xs font-semibold">Member</span>
                        </>
                      )}
                      {!inTeam && (
                        <>
                          <UserPlus size={14} className="text-yellow-500" />
                          <span className="text-yellow-500 text-xs font-semibold">Not Joined</span>
                        </>
                      )}
                    </div>
                  </div>
                  
                  <button
                    onClick={() => inTeam ? navigate(`/teams/${team.id}`) : handleJoin(team)}
                    className={`w-full px-6 py-3 rounded-lg transition font-semibold ${getTeamButtonStyle(team)}`}
                  >
                    {getButtonText()}
                  </button>
                </div>
              );
            })
          )}
        </div>
      </div>
      
      <style>{floatingQuotesCSS}</style>
    </div>
  );
}
