export const API_BASE_URL = 'http://localhost:5232';

export const API_ENDPOINTS = {
  auth: {
    login: '/api/auth/login',
    register: '/api/auth/register',
    logout: '/api/auth/logout',
    me: '/api/auth/me',
    profilePicture: '/api/auth/profile-picture'
  },
  student: {
    dashboard: '/api/student/dashboard',
    proposals: '/api/student/proposals',
    researchAreas: '/api/student/research-areas',
    groups: '/api/student/groups',
    peers: '/api/student/peers'
  },
  supervisor: {
    dashboard: '/api/supervisor/dashboard',
    browse: '/api/supervisor/browse',
    matches: '/api/supervisor/matches',
    expertise: '/api/supervisor/expertise',
    researchAreas: '/api/supervisor/research-areas'
  },
  moduleLeader: {
    dashboard: '/api/moduleleader/dashboard',
    matches: '/api/moduleleader/matches',
    users: '/api/moduleleader/users',
    researchAreas: '/api/moduleleader/research-areas',
    reassignments: '/api/moduleleader/reassignments'
  },
  admin: {
    dashboard: '/api/admin/dashboard',
    users: '/api/admin/users',
    migrations: '/api/admin/migrations',
    auditLogs: '/api/admin/audit-logs'
  }
} as const;
