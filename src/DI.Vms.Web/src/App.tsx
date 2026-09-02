import { Route, Routes } from 'react-router-dom';
import { AppLayout } from './components/AppLayout';
import { AuditLog } from './pages/AuditLog';
import { CheckOut } from './pages/CheckOut';
import { CurrentVisitors } from './pages/CurrentVisitors';
import { Dashboard } from './pages/Dashboard';
import { DiEntities } from './pages/DiEntities';
import { Emergency } from './pages/Emergency';
import { Employees } from './pages/Employees';
import { ExpectedVisitors } from './pages/ExpectedVisitors';
import { Registration } from './pages/Registration';
import { Reports } from './pages/Reports';
import { UsersRoles } from './pages/UsersRoles';
import { VisitorHistory } from './pages/VisitorHistory';

export function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<Dashboard />} />
        <Route path="register" element={<Registration />} />
        <Route path="checkout" element={<CheckOut />} />
        <Route path="current" element={<CurrentVisitors />} />
        <Route path="expected" element={<ExpectedVisitors />} />
        <Route path="emergency" element={<Emergency />} />
        <Route path="history" element={<VisitorHistory />} />
        <Route path="reports" element={<Reports />} />
        <Route path="audit" element={<AuditLog />} />
        <Route path="entities" element={<DiEntities />} />
        <Route path="employees" element={<Employees />} />
        <Route path="users" element={<UsersRoles />} />
        <Route path="*" element={<div className="empty"><strong>Page not found</strong></div>} />
      </Route>
    </Routes>
  );
}
