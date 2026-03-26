import { describe, it, expect } from "vitest";
import auditLogsReducer, {
  loadAuditLogs,
  clearAuditLogs,
} from "../../../store/slices/auditLogsSlice";

describe("auditLogsSlice", () => {
  const initialState = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 25,
    totalPages: 0,
    isLoading: false,
    error: null,
  };

  it("returns initial state", () => {
    const state = auditLogsReducer(undefined, { type: "unknown" });
    expect(state).toEqual(initialState);
  });

  it("handles clearAuditLogs", () => {
    const stateWithData = {
      ...initialState,
      items: [
        {
          auditLogId: "1",
          userId: "2",
          userName: "Test",
          userEmail: "test@example.com",
          timestamp: "2026-01-01T00:00:00Z",
          actionType: "Login",
          resourceType: "Authentication",
          actionDetails: "{}",
          ipAddress: "1.1.1.1",
        },
      ],
      totalCount: 1,
      error: "some error",
    };

    const state = auditLogsReducer(stateWithData, clearAuditLogs());

    expect(state.items).toEqual([]);
    expect(state.totalCount).toBe(0);
    expect(state.error).toBeNull();
  });

  it("sets isLoading true on loadAuditLogs.pending", () => {
    const state = auditLogsReducer(initialState, loadAuditLogs.pending("", {}));
    expect(state.isLoading).toBe(true);
    expect(state.error).toBeNull();
  });

  it("populates items on loadAuditLogs.fulfilled", () => {
    const payload = {
      items: [
        {
          auditLogId: "abc",
          userId: "def",
          userName: "Admin User",
          userEmail: "admin@example.com",
          timestamp: "2026-03-25T10:00:00Z",
          actionType: "Login",
          resourceType: "Authentication",
          actionDetails: "{}",
          ipAddress: "10.0.0.1",
        },
      ],
      totalCount: 1,
      page: 1,
      pageSize: 25,
      totalPages: 1,
    };

    const state = auditLogsReducer(
      { ...initialState, isLoading: true },
      loadAuditLogs.fulfilled(payload, "", {}),
    );

    expect(state.isLoading).toBe(false);
    expect(state.items).toHaveLength(1);
    expect(state.totalCount).toBe(1);
    expect(state.items[0]?.userName).toBe("Admin User");
  });

  it("sets error on loadAuditLogs.rejected", () => {
    const state = auditLogsReducer(
      { ...initialState, isLoading: true },
      loadAuditLogs.rejected(null, "", {}, "Access denied"),
    );

    expect(state.isLoading).toBe(false);
    expect(state.error).toBe("Access denied");
  });
});
