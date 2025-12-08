export const getTimeAgo = (dateString: string): string => {
  const safeString =
    dateString.includes('Z') || dateString.includes('+')
      ? dateString
      : dateString.replace(' ', 'T') + 'Z';

  const now = Date.now();
  const joined = new Date(safeString).getTime();
  const diffMs = Math.max(0, now - joined);

  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return 'Joined just now';
  if (diffMins < 60)
    return `Joined ${diffMins} ${diffMins === 1 ? 'minute' : 'minutes'} ago`;
  if (diffHours < 24)
    return `Joined ${diffHours} ${diffHours === 1 ? 'hour' : 'hours'} ago`;
  return `Joined ${diffDays} ${diffDays === 1 ? 'day' : 'days'} ago`;
};
