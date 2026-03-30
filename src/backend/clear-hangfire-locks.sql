-- Clear Stuck Hangfire Locks
-- Run this in Supabase SQL Editor if you have lock timeout issues

-- 1. Check for stuck locks
SELECT 
    resource,
    acquired AT TIME ZONE 'UTC' as acquired_utc,
    NOW() - acquired as lock_age
FROM hangfire.lock
ORDER BY acquired;

-- 2. Clear all stuck locks (older than 30 minutes)
DELETE FROM hangfire.lock
WHERE acquired < NOW() - INTERVAL '30 minutes';

-- 3. Clear specific recurring job lock (if needed)
DELETE FROM hangfire.lock
WHERE resource = 'hangfire:lock:recurring-job:reminder-scheduler';

-- 4. Clear waitlist job lock (if needed)
DELETE FROM hangfire.lock
WHERE resource = 'hangfire:lock:recurring-job:waitlist-slot-detection';

-- 5. Clear ALL locks (nuclear option - use if above doesn't work)
-- TRUNCATE TABLE hangfire.lock;

-- 6. Verify locks are cleared
SELECT COUNT(*) as remaining_locks
FROM hangfire.lock;
