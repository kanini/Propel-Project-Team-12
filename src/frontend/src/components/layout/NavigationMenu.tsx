/**
 * Navigation Menu Component (NFR-006 RBAC)
 * 
 * Displays role-filtered navigation menu items with active state highlighting.
 * Integrates with navigationConfig.ts to show only items available to the current user's role.
 */

import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { getMenuItemsForRole, groupItemsBySection } from '../../utils/navigationConfig';
import { getUserRole } from '../../utils/tokenStorage';

export const NavigationMenu: React.FC = () => {
  const location = useLocation();
  const role = getUserRole();

  if (!role) {
    return null;
  }

  const menuItems = getMenuItemsForRole(role);
  const groupedItems = groupItemsBySection(menuItems);

  return (
    <nav className="flex-1 space-y-6">
      {Object.entries(groupedItems).map(([section, items]) => (
        <div key={section}>
          {section !== 'main' && (
            <h3 className="px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
              {section}
            </h3>
          )}
          <div className="space-y-1">
            {items.map((item) => {
              const isActive = location.pathname === item.path;
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  className={`flex items-center px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
                    isActive
                      ? 'bg-blue-50 text-blue-700'
                      : 'text-gray-700 hover:bg-gray-100 hover:text-gray-900'
                  }`}
                >
                  <span>{item.label}</span>
                </Link>
              );
            })}
          </div>
        </div>
      ))}
    </nav>
  );
};
