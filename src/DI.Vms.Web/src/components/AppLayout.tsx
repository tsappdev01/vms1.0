import { NavLink, Outlet } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { usingMock } from '../api';
import { useCurrentUser } from '../api/hooks';
import { titleCase } from '../lib/format';

const NAV = [
  {
    group: 'Reception',
    items: [
      { to: '/register', label: 'New Visitor' },
      { to: '/checkout', label: 'Check Out' },
    ],
  },
  {
    group: 'Monitoring',
    items: [
      { to: '/', label: 'Dashboard', end: true },
      { to: '/current', label: 'Current Visitors' },
      { to: '/expected', label: 'Expected Visitors' },
      { to: '/emergency', label: 'Emergency', emergency: true },
    ],
  },
  {
    group: 'Records',
    items: [
      { to: '/history', label: 'Visitor History' },
      { to: '/reports', label: 'Reports' },
      { to: '/audit', label: 'Audit Log' },
    ],
  },
  {
    group: 'Administration',
    items: [
      { to: '/entities', label: 'DI Entities' },
      { to: '/employees', label: 'Employees / Hosts' },
      { to: '/users', label: 'Users & Roles' },
    ],
  },
];

export function AppLayout() {
  const { data: user } = useCurrentUser();

  return (
    <div className="app">
      <nav className="sidebar">
        <div className="brand">
          Dubai Investments
          <span>Visitor Management</span>
        </div>

        {NAV.map((g) => (
          <div key={g.group}>
            <div className="nav-group-label">{g.group}</div>
            {g.items.map((i) => (
              <NavLink
                key={i.to}
                to={i.to}
                end={'end' in i ? i.end : false}
                className={({ isActive }) =>
                  ['nav-link', 'emergency' in i && i.emergency ? 'emergency' : '', isActive ? 'active' : '']
                    .filter(Boolean).join(' ')
                }
              >
                {i.label}
              </NavLink>
            ))}
          </div>
        ))}
      </nav>

      <div className="main">
        <header className="topbar">
          <div style={{ fontSize: 12.5, color: 'var(--text-secondary)' }}>
            DIP Office · Times shown in Gulf Standard Time
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            {user && (
              <span style={{ fontSize: 12.5, color: 'var(--text-secondary)' }}>
                {user.name} · <span className="chip">{titleCase(user.role)}</span>
              </span>
            )}
            <ThemeToggle />
          </div>
        </header>

        <main className="content">
          {usingMock && (
            <div className="banner warn no-print">
              <strong>Sample data.</strong>
              <span>
                These screens are served from in-memory fixtures, not the database. Set{' '}
                <code>VITE_USE_MOCK=false</code> to use DI.Vms.Api.
              </span>
            </div>
          )}
          <Outlet />
        </main>
      </div>
    </div>
  );
}

function ThemeToggle() {
  const [theme, setTheme] = useState<'light' | 'dark' | 'system'>('system');

  useEffect(() => {
    const root = document.documentElement;
    if (theme === 'system') root.removeAttribute('data-theme');
    else root.setAttribute('data-theme', theme);
  }, [theme]);

  return (
    <select
      value={theme}
      onChange={(e) => setTheme(e.target.value as typeof theme)}
      aria-label="Colour theme"
      style={{ fontSize: 12 }}
    >
      <option value="system">System theme</option>
      <option value="light">Light</option>
      <option value="dark">Dark</option>
    </select>
  );
}
