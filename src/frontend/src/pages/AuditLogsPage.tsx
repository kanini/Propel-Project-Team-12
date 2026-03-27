import React, { useEffect, useState, useCallback } from "react";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { loadAuditLogs } from "../store/slices/auditLogsSlice";

const ACTION_TYPE_OPTIONS = [
  "Login",
  "Logout",
  "FailedLogin",
  "SessionTimeout",
  "Registration",
  "EmailVerified",
  "AccountLocked",
  "PasswordChanged",
];

const ACTION_TYPE_COLORS: Record<string, string> = {
  Login: "bg-green-100 text-green-800",
  Logout: "bg-gray-100 text-gray-800",
  FailedLogin: "bg-red-100 text-red-800",
  SessionTimeout: "bg-amber-100 text-amber-800",
  Registration: "bg-blue-100 text-blue-800",
  EmailVerified: "bg-teal-100 text-teal-800",
  AccountLocked: "bg-red-100 text-red-800",
  PasswordChanged: "bg-purple-100 text-purple-800",
};

/**
 * Admin Audit Logs page (US_022, US_055, SCR-025).
 * Displays searchable, filterable table of audit events.
 */
const AuditLogsPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { items, totalCount, page, pageSize, totalPages, isLoading, error } =
    useAppSelector((state) => state.auditLogs);

  const [actionTypeFilter, setActionTypeFilter] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  const fetchLogs = useCallback(
    (pageNum: number = 1) => {
      dispatch(
        loadAuditLogs({
          actionType: actionTypeFilter || undefined,
          startDate: startDate || undefined,
          endDate: endDate || undefined,
          page: pageNum,
          pageSize: 25,
        }),
      );
    },
    [dispatch, actionTypeFilter, startDate, endDate],
  );

  useEffect(() => {
    fetchLogs(1);
  }, [fetchLogs]);

  const handlePageChange = (newPage: number) => {
    if (newPage >= 1 && newPage <= totalPages) {
      fetchLogs(newPage);
    }
  };

  const handleClearFilters = () => {
    setActionTypeFilter("");
    setSearchTerm("");
    setStartDate("");
    setEndDate("");
  };

  const formatTimestamp = (ts: string) => {
    return new Date(ts).toLocaleString();
  };

  // Client-side search filtering on displayed items
  const filteredItems = searchTerm
    ? items.filter(
        (item) =>
          item.userName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          item.userEmail?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          item.actionType.toLowerCase().includes(searchTerm.toLowerCase()) ||
          item.ipAddress?.toLowerCase().includes(searchTerm.toLowerCase()),
      )
    : items;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Audit Logs</h1>
          <p className="mt-1 text-sm text-gray-500">
            {totalCount} total entries
          </p>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow p-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Search */}
          <div>
            <label
              htmlFor="audit-search"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              Search
            </label>
            <input
              id="audit-search"
              type="text"
              placeholder="Search user, action, IP..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Action Type */}
          <div>
            <label
              htmlFor="action-type-filter"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              Action Type
            </label>
            <select
              id="action-type-filter"
              value={actionTypeFilter}
              onChange={(e) => setActionTypeFilter(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All Actions</option>
              {ACTION_TYPE_OPTIONS.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>

          {/* Start Date */}
          <div>
            <label
              htmlFor="start-date"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              From
            </label>
            <input
              id="start-date"
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* End Date */}
          <div>
            <label
              htmlFor="end-date"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              To
            </label>
            <input
              id="end-date"
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        {/* Clear Filters */}
        {(actionTypeFilter || searchTerm || startDate || endDate) && (
          <div className="mt-3">
            <button
              onClick={handleClearFilters}
              className="text-sm text-blue-600 hover:text-blue-800"
            >
              Clear all filters
            </button>
          </div>
        )}
      </div>

      {/* Error State */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-700">
          {error}
        </div>
      )}

      {/* Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Timestamp
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  User
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Action
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  IP Address
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Details
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {isLoading ? (
                <tr>
                  <td
                    colSpan={5}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    <div className="flex items-center justify-center gap-2">
                      <svg
                        className="animate-spin h-5 w-5 text-blue-500"
                        viewBox="0 0 24 24"
                        fill="none"
                      >
                        <circle
                          className="opacity-25"
                          cx="12"
                          cy="12"
                          r="10"
                          stroke="currentColor"
                          strokeWidth="4"
                        />
                        <path
                          className="opacity-75"
                          fill="currentColor"
                          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                        />
                      </svg>
                      Loading audit logs...
                    </div>
                  </td>
                </tr>
              ) : filteredItems.length === 0 ? (
                <tr>
                  <td
                    colSpan={5}
                    className="px-6 py-12 text-center text-gray-500"
                  >
                    No audit log entries found.
                  </td>
                </tr>
              ) : (
                filteredItems.map((entry) => (
                  <tr key={entry.auditLogId} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatTimestamp(entry.timestamp)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      {entry.userName ? (
                        <div>
                          <div className="font-medium text-gray-900">
                            {entry.userName}
                          </div>
                          <div className="text-gray-500 text-xs">
                            {entry.userEmail}
                          </div>
                        </div>
                      ) : (
                        <span className="text-gray-400 italic">System</span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                          ACTION_TYPE_COLORS[entry.actionType] ??
                          "bg-gray-100 text-gray-800"
                        }`}
                      >
                        {entry.actionType}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 font-mono">
                      {entry.ipAddress ?? "—"}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500 max-w-xs truncate">
                      {entry.actionDetails !== "{}" ? entry.actionDetails : "—"}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
            <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
              <p className="text-sm text-gray-700">
                Showing{" "}
                <span className="font-medium">
                  {(page - 1) * pageSize + 1}
                </span>{" "}
                to{" "}
                <span className="font-medium">
                  {Math.min(page * pageSize, totalCount)}
                </span>{" "}
                of <span className="font-medium">{totalCount}</span> entries
              </p>
              <nav
                className="inline-flex -space-x-px rounded-md shadow-sm"
                aria-label="Pagination"
              >
                <button
                  onClick={() => handlePageChange(page - 1)}
                  disabled={page <= 1}
                  className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <span className="sr-only">Previous</span>
                  <svg
                    className="h-5 w-5"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                  >
                    <path
                      fillRule="evenodd"
                      d="M11.78 5.22a.75.75 0 0 1 0 1.06L8.06 10l3.72 3.72a.75.75 0 1 1-1.06 1.06l-4.25-4.25a.75.75 0 0 1 0-1.06l4.25-4.25a.75.75 0 0 1 1.06 0Z"
                      clipRule="evenodd"
                    />
                  </svg>
                </button>
                {(() => {
                  const pages: (number | string)[] = [];
                  const start = Math.max(2, page - 2);
                  const end = Math.min(totalPages - 1, page + 2);
                  pages.push(1);
                  if (start > 2) pages.push("start-ellipsis");
                  for (let i = start; i <= end; i++) pages.push(i);
                  if (end < totalPages - 1) pages.push("end-ellipsis");
                  if (totalPages > 1) pages.push(totalPages);
                  return pages.map((pg) =>
                    typeof pg === "string" ? (
                      <span
                        key={pg}
                        className="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-gray-700 ring-1 ring-inset ring-gray-300"
                      >
                        &hellip;
                      </span>
                    ) : (
                      <button
                        key={pg}
                        onClick={() => handlePageChange(pg)}
                        className={`relative inline-flex items-center px-4 py-2 text-sm font-semibold ring-1 ring-inset ring-gray-300 ${
                          pg === page
                            ? "z-10 bg-blue-600 text-white focus-visible:outline-blue-600"
                            : "text-gray-900 hover:bg-gray-50"
                        }`}
                      >
                        {pg}
                      </button>
                    ),
                  );
                })()}
                <button
                  onClick={() => handlePageChange(page + 1)}
                  disabled={page >= totalPages}
                  className="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <span className="sr-only">Next</span>
                  <svg
                    className="h-5 w-5"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                  >
                    <path
                      fillRule="evenodd"
                      d="M8.22 5.22a.75.75 0 0 1 1.06 0l4.25 4.25a.75.75 0 0 1 0 1.06l-4.25 4.25a.75.75 0 0 1-1.06-1.06L11.94 10 8.22 6.28a.75.75 0 0 1 0-1.06Z"
                      clipRule="evenodd"
                    />
                  </svg>
                </button>
              </nav>
            </div>

            {/* Mobile pagination */}
            <div className="flex flex-1 justify-between sm:hidden">
              <button
                onClick={() => handlePageChange(page - 1)}
                disabled={page <= 1}
                className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="inline-flex items-center text-sm text-gray-700">
                Page {page} of {totalPages}
              </span>
              <button
                onClick={() => handlePageChange(page + 1)}
                disabled={page >= totalPages}
                className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default AuditLogsPage;
