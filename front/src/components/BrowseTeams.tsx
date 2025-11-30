import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Users, Lock, Crown, Shield, Filter, ArrowUpDown, User as UserIcon, UserPlus } from 'lucide-react';
import api from '../services/api.service';
import type { Team, User } from '../services/api.service';
import { renderPulsingStar, floatingQuotesCSS } from '../utils/praises';
import { useToast } from '../contexts/ToastContext';

interface BrowseTeamsProps {
  userName: string;
  userId: number;
  onUserCreated: (user: User) => void;
}

type FilterType = 'all' | 'joined' | 'public' | 'private' | 'moderator' | 'owner';
type SortType = 'alpha' | 'newest' | 'oldest' | 'most-members' | 'least-members' | 'by-role';

export function BrowseTeams({ userName, userId, onUserCreated }: BrowseTeamsProps) {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [teams, setTeams] = useState<Team[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [filter, setFilter] = useState<FilterType>('all');
  const [sort, setSort] = useState<SortType>('newest');

  useEffect(() => {
    fetchTeams();
    
    // Log user activity for analytics
    console.log(`User ${userName} (ID: ${userId}) browsing teams`);
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

  const handleJoin = async (team: Team) => {
    try {
      // Check if user already in team
      const existingUser = team.users?.find(u => u.name === userName);
      
      if (existingUser) {
        // User already exists - use existing user
        onUserCreated(existingUser);
        navigate(`/teams/${team.id}`);
        return;
      }
      
      // User doesn't exist - create new
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

  // Get user's role in a team
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
  
  // Get priority number for role-based sorting
  const getRolePriority = (team: Team): number => {
    const role = getUserRole(team);
    
    if (role === 'owner') return 1;      // Owner (highest priority)
    if (role === 'moderator') return 2;  // Moderator
    if (role === 'member') return 3;     // Member
    return 4;                            // Not joined (lowest priority)
  };

  // Filter teams
  const getFilteredTeams = (): Team[] => {
    let filtered = teams;

    switch (filter) {
      case 'joined':
        filtered = teams.filter(t => isUserInTeam(t));
        break;
      case 'public':
        filtered = teams.filter(t => !t.isPrivate);
        break;
      case 'private':
        // Show private teams only if user is a member
        filtered = teams.filter(t => t.isPrivate && isUserInTeam(t));
        break;
      case 'moderator':
        filtered = teams.filter(t => getUserRole(t) === 'moderator');
        break;
      case 'owner':
        filtered = teams.filter(t => getUserRole(t) === 'owner');
        break;
      case 'all':
      default:
        // Show all public teams + private teams user has joined
        filtered = teams.filter(t => !t.isPrivate || isUserInTeam(t));
        break;
    }

    return filtered;
  };

  // Simplified sorting function with a clear approach
  const sortTeams = (teams: Team[]): Team[] => {
    const teamsToSort = [...teams];
    
    // Determine if we need to split teams by role first
    const splitByRoleFirst = sort === 'by-role';
    
    // If using role-based sort, first group teams by role
    if (splitByRoleFirst) {
      // First sort by role
      return teamsToSort.sort((a, b) => {
        // Primary sort by role
        const roleA = getRolePriority(a);
        const roleB = getRolePriority(b);
        if (roleA !== roleB) {
          return roleA - roleB;
        }
        
        // When roles are the same, sort alphabetically
        const nameComparison = a.name.localeCompare(b.name);
        if (nameComparison !== 0) {
          return nameComparison;
        }
        
        // If names are also the same, sort by newest first
        const dateComparison = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        if (dateComparison !== 0) {
          return dateComparison;
        }
        
        // Final tie-breaker: most members first
        return (b.users?.length || 0) - (a.users?.length || 0);
      });
    }
    
    // For other sort types, use simpler logic
    return teamsToSort.sort((a, b) => {
      let result = 0;
      
      // First apply the selected sort method
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
      
      // If primary sort yields a difference, use it
      if (result !== 0) {
        return result;
      }
      
      // For ties, apply secondary sorting in a consistent order
      
      // First by role
      const roleA = getRolePriority(a);
      const roleB = getRolePriority(b);
      if (roleA !== roleB) {
        return roleA - roleB;
      }
      
      // Then alphabetically (if not already sorted by name)
      if (sort !== 'alpha') {
        const nameComparison = a.name.localeCompare(b.name);
        if (nameComparison !== 0) {
          return nameComparison;
        }
      }
      
      // Then by date (if not already sorted by date)
      if (sort !== 'newest' && sort !== 'oldest') {
        const dateComparison = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        if (dateComparison !== 0) {
          return dateComparison;
        }
      }
      
      // Finally by member count (if not already sorted by members)
      if (sort !== 'most-members' && sort !== 'least-members') {
        const memberComparison = (b.users?.length || 0) - (a.users?.length || 0);
        if (memberComparison !== 0) {
          return memberComparison;
        }
      }
      
      // Use team ID as final deterministic tie-breaker
      return a.id - b.id;
    });
  };
  
  // Sort teams
  const getSortedTeams = (filtered: Team[]): Team[] => {
    return sortTeams(filtered);
  };

  const displayTeams = getSortedTeams(getFilteredTeams());

  // Get button styling based on role
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

  return (
    <div className="min-h-screen bg-slate-950 p-8">
      <button 
        onClick={() => navigate('/menu')} 
        className="text-slate-400 hover:text-white mb-8 flex items-center gap-2 transition"
      >
        <ArrowLeft size={20} />
        Back
      </button>
      
      <h1 className="text-4xl font-bold text-white mb-2 text-center">
        Browse Teams{renderPulsingStar({ className: 'text-yellow-400' })}
      </h1>
      
      {/* Filter and Sort Controls */}
      <div className="max-w-6xl mx-auto mb-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Filter */}
          <div className="bg-slate-900 p-4 rounded-lg border border-slate-800">
            <div className="flex items-center gap-2 mb-3">
              <Filter size={18} className="text-yellow-500" />
              <h3 className="text-white font-semibold">Filter</h3>
            </div>
            <div className="grid grid-cols-3 gap-2">
              <button
                onClick={() => setFilter('all')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  filter === 'all'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                All
              </button>
              <button
                onClick={() => setFilter('joined')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  filter === 'joined'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                Joined
              </button>
              <button
                onClick={() => setFilter('public')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  filter === 'public'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                Public
              </button>
              <button
                onClick={() => setFilter('private')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  filter === 'private'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                Private
              </button>
              <button
                onClick={() => setFilter('moderator')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  filter === 'moderator'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                Moderator
              </button>
              <button
                onClick={() => setFilter('owner')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  filter === 'owner'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                Owner
              </button>
            </div>
          </div>

          {/* Sort */}
          <div className="bg-slate-900 p-4 rounded-lg border border-slate-800">
            <div className="flex items-center gap-2 mb-3">
              <ArrowUpDown size={18} className="text-yellow-500" />
              <h3 className="text-white font-semibold">Sort by</h3>
            </div>
            <div className="grid grid-cols-2 gap-2">
              <button
                onClick={() => setSort('alpha')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  sort === 'alpha'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                A-Z
              </button>
              <button
                onClick={() => setSort('newest')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  sort === 'newest'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                Newest
              </button>
              <button
                onClick={() => setSort('oldest')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  sort === 'oldest'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                Oldest
              </button>
              <button
                onClick={() => setSort('most-members')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  sort === 'most-members'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                Most Members
              </button>
              <button
                onClick={() => setSort('least-members')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  sort === 'least-members'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                Least Members
              </button>
              <button
                onClick={() => setSort('by-role')}
                className={`px-3 py-2 rounded text-sm font-medium transition ${
                  sort === 'by-role'
                    ? 'bg-yellow-500 text-black'
                    : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                }`}
              >
                By Role
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Results count */}
      <p className="text-slate-400 text-center mb-8">
        {loading ? 'Loading...' : `${displayTeams.length} ${displayTeams.length === 1 ? 'team' : 'teams'}`}
      </p>
      
      {/* Teams grid */}
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

                    {/* Role badge */}
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
