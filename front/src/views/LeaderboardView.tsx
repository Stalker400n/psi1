import { useState, useEffect } from 'react';
import { Star } from 'lucide-react';
import api from '../services/api.service';
import type { User } from '../services/api.service';

interface LeaderboardViewProps {
  teamId: number;
}

export function LeaderboardView({ teamId }: LeaderboardViewProps) {
  const [users, setUsers] = useState<User[]>([]);

  useEffect(() => {
    fetchUsers();
    const interval = setInterval(fetchUsers, 3000);
    return () => clearInterval(interval);
  }, [teamId]);

  const fetchUsers = async () => {
    try {
      const data = await api.usersApi.getAll(teamId);
      setUsers(data.sort((a: User, b: User) => b.score - a.score));
    } catch (error) {
      console.error('Error fetching users:', error);
    }
  };

  return (
    <div className="bg-slate-900 rounded-lg p-6">
      <h2 className="text-xl font-semibold text-white mb-4">Leaderboard</h2>
      <div className="space-y-3">
        {users.map((user, idx) => (
          <div key={user.id} className="bg-slate-800 p-4 rounded-lg flex justify-between items-center">
            <div className="flex items-center gap-4">
              <span className="text-2xl font-bold text-slate-600">#{idx + 1}</span>
              <div>
                <p className="text-white font-semibold">{user.name}</p>
                <p className="text-slate-400 text-sm">{user.isActive ? 'Active' : 'Inactive'}</p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <Star size={20} className="text-yellow-500" />
              <span className="text-white text-xl font-bold">{user.score}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
