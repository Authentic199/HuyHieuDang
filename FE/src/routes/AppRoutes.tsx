import { lazy } from 'react';
import { Route, Routes } from 'react-router-dom';

import { AppLayout } from '../layouts/AppLayout';
import { paths } from './paths';
import { RequireAuth } from './RequireAuth';

// Tách gói theo màn để lần mở đầu nhẹ.
const LoginPage = lazy(() => import('../pages/login/LoginPage'));
const DashboardPage = lazy(() => import('../pages/dashboard/DashboardPage'));
const MembersPage = lazy(() => import('../pages/members/MembersPage'));
const PeriodsPage = lazy(() => import('../pages/periods/PeriodsPage'));
const UncoveredPage = lazy(() => import('../pages/uncovered/UncoveredPage'));
const SettingsPage = lazy(() => import('../pages/settings/SettingsPage'));
const NotFoundPage = lazy(() => import('../pages/NotFoundPage'));

export function AppRoutes() {
  return (
    <Routes>
      <Route path={paths.login} element={<LoginPage />} />
      <Route element={<RequireAuth />}>
        <Route element={<AppLayout />}>
          <Route path={paths.dashboard} element={<DashboardPage />} />
          <Route path={paths.members} element={<MembersPage />} />
          <Route path={paths.periods} element={<PeriodsPage />} />
          <Route path={paths.uncovered} element={<UncoveredPage />} />
          <Route path={paths.settings} element={<SettingsPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Route>
    </Routes>
  );
}
