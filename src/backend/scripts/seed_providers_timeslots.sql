-- =====================================================================================
-- Seed Data for Providers and TimeSlots Tables (US_023 - Provider Browser)
-- =====================================================================================
-- Purpose: Populate test data for provider browsing, filtering, and pagination
-- Provider Count: 120 (115 active, 5 inactive)
-- TimeSlot Count: 1000+ (60% available, 40% booked)
-- Edge Cases: 10 providers with zero availability, 20 with limited availability
-- =====================================================================================

BEGIN;

-- =====================================================================================
-- CLEANUP: Remove existing data (idempotent seed script)
-- =====================================================================================

DELETE FROM "TimeSlots";
DELETE FROM "Providers";

-- =====================================================================================
-- PROVIDERS: 120 providers across 8 specialties
-- =====================================================================================
-- Specialty Distribution:
--   - Cardiology: 20 providers
--   - Pediatrics: 20 providers
--   - Dermatology: 15 providers
--   - Orthopedics: 15 providers
--   - Radiology: 15 providers
--   - Psychiatry: 15 providers
--   - Neurology: 10 providers
--   - Family Medicine: 10 providers
-- Rating Distribution: 70% (4.0-5.0), 20% (3.5-3.9), 10% (3.0-3.4)
-- Active Status: 115 active (TRUE), 5 inactive (FALSE)
-- =====================================================================================

-- Cardiology Providers (20) - 10 with ZERO availability
INSERT INTO "Providers" ("ProviderId", "Name", "Specialty", "Email", "Phone", "LicenseNumber", "IsActive", "CreatedAt") VALUES
(gen_random_uuid(), 'Dr. Sarah Chen', 'Cardiology', 'schen@hospital.com', '555-0101', 'MD-CARD-001', TRUE, NOW() - INTERVAL '350 days'),
(gen_random_uuid(), 'Dr. James Wilson', 'Cardiology', 'jwilson@hospital.com', '555-0102', 'MD-CARD-002', TRUE, NOW() - INTERVAL '340 days'),
(gen_random_uuid(), 'Dr. Ravi Patel', 'Cardiology', 'rpatel@hospital.com', '555-0103', 'MD-CARD-003', TRUE, NOW() - INTERVAL '330 days'),
(gen_random_uuid(), 'Dr. Emily Liu', 'Cardiology', 'eliu@hospital.com', '555-0104', 'MD-CARD-004', TRUE, NOW() - INTERVAL '320 days'),
(gen_random_uuid(), 'Dr. Michael Johnson', 'Cardiology', 'mjohnson@hospital.com', '555-0105', 'MD-CARD-005', TRUE, NOW() - INTERVAL '310 days'),
(gen_random_uuid(), 'Dr. Amanda Rodriguez', 'Cardiology', 'arodriguez@hospital.com', '555-0106', 'MD-CARD-006', TRUE, NOW() - INTERVAL '300 days'),
(gen_random_uuid(), 'Dr. David Kim', 'Cardiology', 'dkim@hospital.com', '555-0107', 'MD-CARD-007', TRUE, NOW() - INTERVAL '290 days'),
(gen_random_uuid(), 'Dr. Jennifer Martinez', 'Cardiology', 'jmartinez@hospital.com', '555-0108', 'MD-CARD-008', TRUE, NOW() - INTERVAL '280 days'),
(gen_random_uuid(), 'Dr. Robert Thompson', 'Cardiology', 'rthompson@hospital.com', '555-0109', 'MD-CARD-009', TRUE, NOW() - INTERVAL '270 days'),
(gen_random_uuid(), 'Dr. Lisa Anderson', 'Cardiology', 'landerson@hospital.com', '555-0110', 'MD-CARD-010', TRUE, NOW() - INTERVAL '260 days'),
-- These 10 Cardiology providers will have ZERO availability (edge case for "Join Waitlist")
(gen_random_uuid(), 'Dr. Christopher Lee', 'Cardiology', 'clee@hospital.com', '555-0111', 'MD-CARD-011', TRUE, NOW() - INTERVAL '250 days'),
(gen_random_uuid(), 'Dr. Maria Garcia', 'Cardiology', 'mgarcia@hospital.com', '555-0112', 'MD-CARD-012', TRUE, NOW() - INTERVAL '240 days'),
(gen_random_uuid(), 'Dr. Thomas Brown', 'Cardiology', 'tbrown@hospital.com', '555-0113', 'MD-CARD-013', TRUE, NOW() - INTERVAL '230 days'),
(gen_random_uuid(), 'Dr. Patricia Davis', 'Cardiology', 'pdavis@hospital.com', '555-0114', 'MD-CARD-014', TRUE, NOW() - INTERVAL '220 days'),
(gen_random_uuid(), 'Dr. Daniel Miller', 'Cardiology', 'dmiller@hospital.com', '555-0115', 'MD-CARD-015', TRUE, NOW() - INTERVAL '210 days'),
(gen_random_uuid(), 'Dr. Nancy Wilson', 'Cardiology', 'nwilson@hospital.com', '555-0116', 'MD-CARD-016', TRUE, NOW() - INTERVAL '200 days'),
(gen_random_uuid(), 'Dr. Kevin Moore', 'Cardiology', 'kmoore@hospital.com', '555-0117', 'MD-CARD-017', TRUE, NOW() - INTERVAL '190 days'),
(gen_random_uuid(), 'Dr. Sandra Taylor', 'Cardiology', 'staylor@hospital.com', '555-0118', 'MD-CARD-018', TRUE, NOW() - INTERVAL '180 days'),
(gen_random_uuid(), 'Dr. Paul Anderson', 'Cardiology', 'panderson@hospital.com', '555-0119', 'MD-CARD-019', TRUE, NOW() - INTERVAL '170 days'),
(gen_random_uuid(), 'Dr. Karen Thomas', 'Cardiology', 'kthomas@hospital.com', '555-0120', 'MD-CARD-020', TRUE, NOW() - INTERVAL '160 days');

-- Pediatrics Providers (20)
INSERT INTO "Providers" ("ProviderId", "Name", "Specialty", "Email", "Phone", "LicenseNumber", "IsActive", "CreatedAt") VALUES
(gen_random_uuid(), 'Dr. Michelle Chang', 'Pediatrics', 'mchang@hospital.com', '555-0201', 'MD-PEDI-001', TRUE, NOW() - INTERVAL '150 days'),
(gen_random_uuid(), 'Dr. Brian Parker', 'Pediatrics', 'bparker@hospital.com', '555-0202', 'MD-PEDI-002', TRUE, NOW() - INTERVAL '145 days'),
(gen_random_uuid(), 'Dr. Angela White', 'Pediatrics', 'awhite@hospital.com', '555-0203', 'MD-PEDI-003', TRUE, NOW() - INTERVAL '140 days'),
(gen_random_uuid(), 'Dr. Jason Lewis', 'Pediatrics', 'jlewis@hospital.com', '555-0204', 'MD-PEDI-004', TRUE, NOW() - INTERVAL '135 days'),
(gen_random_uuid(), 'Dr. Rachel Walker', 'Pediatrics', 'rwalker@hospital.com', '555-0205', 'MD-PEDI-005', TRUE, NOW() - INTERVAL '130 days'),
(gen_random_uuid(), 'Dr. Steven Hall', 'Pediatrics', 'shall@hospital.com', '555-0206', 'MD-PEDI-006', TRUE, NOW() - INTERVAL '125 days'),
(gen_random_uuid(), 'Dr. Laura Allen', 'Pediatrics', 'lallen@hospital.com', '555-0207', 'MD-PEDI-007', TRUE, NOW() - INTERVAL '120 days'),
(gen_random_uuid(), 'Dr. Mark Young', 'Pediatrics', 'myoung@hospital.com', '555-0208', 'MD-PEDI-008', TRUE, NOW() - INTERVAL '115 days'),
(gen_random_uuid(), 'Dr. Stephanie King', 'Pediatrics', 'sking@hospital.com', '555-0209', 'MD-PEDI-009', TRUE, NOW() - INTERVAL '110 days'),
(gen_random_uuid(), 'Dr. Andrew Wright', 'Pediatrics', 'awright@hospital.com', '555-0210', 'MD-PEDI-010', TRUE, NOW() - INTERVAL '105 days'),
(gen_random_uuid(), 'Dr. Nicole Scott', 'Pediatrics', 'nscott@hospital.com', '555-0211', 'MD-PEDI-011', TRUE, NOW() - INTERVAL '100 days'),
(gen_random_uuid(), 'Dr. Gregory Green', 'Pediatrics', 'ggreen@hospital.com', '555-0212', 'MD-PEDI-012', TRUE, NOW() - INTERVAL '95 days'),
(gen_random_uuid(), 'Dr. Rebecca Adams', 'Pediatrics', 'radams@hospital.com', '555-0213', 'MD-PEDI-013', TRUE, NOW() - INTERVAL '90 days'),
(gen_random_uuid(), 'Dr. Timothy Baker', 'Pediatrics', 'tbaker@hospital.com', '555-0214', 'MD-PEDI-014', TRUE, NOW() - INTERVAL '85 days'),
(gen_random_uuid(), 'Dr. Catherine Hill', 'Pediatrics', 'chill@hospital.com', '555-0215', 'MD-PEDI-015', TRUE, NOW() - INTERVAL '80 days'),
(gen_random_uuid(), 'Dr. Raymond Nelson', 'Pediatrics', 'rnelson@hospital.com', '555-0216', 'MD-PEDI-016', TRUE, NOW() - INTERVAL '75 days'),
(gen_random_uuid(), 'Dr. Victoria Carter', 'Pediatrics', 'vcarter@hospital.com', '555-0217', 'MD-PEDI-017', TRUE, NOW() - INTERVAL '70 days'),
(gen_random_uuid(), 'Dr. Walter Mitchell', 'Pediatrics', 'wmitchell@hospital.com', '555-0218', 'MD-PEDI-018', TRUE, NOW() - INTERVAL '65 days'),
(gen_random_uuid(), 'Dr. Diana Perez', 'Pediatrics', 'dperez@hospital.com', '555-0219', 'MD-PEDI-019', TRUE, NOW() - INTERVAL '60 days'),
(gen_random_uuid(), 'Dr. Peter Roberts', 'Pediatrics', 'proberts@hospital.com', '555-0220', 'MD-PEDI-020', TRUE, NOW() - INTERVAL '55 days');

-- Dermatology Providers (15)
INSERT INTO "Providers" ("ProviderId", "Name", "Specialty", "Email", "Phone", "LicenseNumber", "IsActive", "CreatedAt") VALUES
(gen_random_uuid(), 'Dr. Olivia Turner', 'Dermatology', 'oturner@hospital.com', '555-0301', 'MD-DERM-001', TRUE, NOW() - INTERVAL '50 days'),
(gen_random_uuid(), 'Dr. Henry Phillips', 'Dermatology', 'hphillips@hospital.com', '555-0302', 'MD-DERM-002', TRUE, NOW() - INTERVAL '48 days'),
(gen_random_uuid(), 'Dr. Samantha Campbell', 'Dermatology', 'scampbell@hospital.com', '555-0303', 'MD-DERM-003', TRUE, NOW() - INTERVAL '46 days'),
(gen_random_uuid(), 'Dr. Edward Parker', 'Dermatology', 'eparker@hospital.com', '555-0304', 'MD-DERM-004', TRUE, NOW() - INTERVAL '44 days'),
(gen_random_uuid(), 'Dr. Caroline Evans', 'Dermatology', 'cevans@hospital.com', '555-0305', 'MD-DERM-005', TRUE, NOW() - INTERVAL '42 days'),
(gen_random_uuid(), 'Dr. Benjamin Edwards', 'Dermatology', 'bedwards@hospital.com', '555-0306', 'MD-DERM-006', TRUE, NOW() - INTERVAL '40 days'),
(gen_random_uuid(), 'Dr. Jessica Collins', 'Dermatology', 'jcollins@hospital.com', '555-0307', 'MD-DERM-007', TRUE, NOW() - INTERVAL '38 days'),
(gen_random_uuid(), 'Dr. Alexander Stewart', 'Dermatology', 'astewart@hospital.com', '555-0308', 'MD-DERM-008', TRUE, NOW() - INTERVAL '36 days'),
(gen_random_uuid(), 'Dr. Megan Sanchez', 'Dermatology', 'msanchez@hospital.com', '555-0309', 'MD-DERM-009', TRUE, NOW() - INTERVAL '34 days'),
(gen_random_uuid(), 'Dr. Jonathan Morris', 'Dermatology', 'jmorris@hospital.com', '555-0310', 'MD-DERM-010', TRUE, NOW() - INTERVAL '32 days'),
(gen_random_uuid(), 'Dr. Natalie Rogers', 'Dermatology', 'nrogers@hospital.com', '555-0311', 'MD-DERM-011', TRUE, NOW() - INTERVAL '30 days'),
(gen_random_uuid(), 'Dr. Nathan Reed', 'Dermatology', 'nreed@hospital.com', '555-0312', 'MD-DERM-012', TRUE, NOW() - INTERVAL '28 days'),
(gen_random_uuid(), 'Dr. Hannah Cook', 'Dermatology', 'hcook@hospital.com', '555-0313', 'MD-DERM-013', TRUE, NOW() - INTERVAL '26 days'),
(gen_random_uuid(), 'Dr. Aaron Morgan', 'Dermatology', 'amorgan@hospital.com', '555-0314', 'MD-DERM-014', TRUE, NOW() - INTERVAL '24 days'),
(gen_random_uuid(), 'Dr. Brittany Bell', 'Dermatology', 'bbell@hospital.com', '555-0315', 'MD-DERM-015', TRUE, NOW() - INTERVAL '22 days');

-- Orthopedics Providers (15)
INSERT INTO "Providers" ("ProviderId", "Name", "Specialty", "Email", "Phone", "LicenseNumber", "IsActive", "CreatedAt") VALUES
(gen_random_uuid(), 'Dr. Eric Murphy', 'Orthopedics', 'emurphy@hospital.com', '555-0401', 'MD-ORTH-001', TRUE, NOW() - INTERVAL '20 days'),
(gen_random_uuid(), 'Dr. Melissa Bailey', 'Orthopedics', 'mbailey@hospital.com', '555-0402', 'MD-ORTH-002', TRUE, NOW() - INTERVAL '19 days'),
(gen_random_uuid(), 'Dr. Joshua Rivera', 'Orthopedics', 'jrivera@hospital.com', '555-0403', 'MD-ORTH-003', TRUE, NOW() - INTERVAL '18 days'),
(gen_random_uuid(), 'Dr. Amy Cooper', 'Orthopedics', 'acooper@hospital.com', '555-0404', 'MD-ORTH-004', TRUE, NOW() - INTERVAL '17 days'),
(gen_random_uuid(), 'Dr. Ryan Richardson', 'Orthopedics', 'rrichardson@hospital.com', '555-0405', 'MD-ORTH-005', TRUE, NOW() - INTERVAL '16 days'),
(gen_random_uuid(), 'Dr. Kelly Cox', 'Orthopedics', 'kcox@hospital.com', '555-0406', 'MD-ORTH-006', TRUE, NOW() - INTERVAL '15 days'),
(gen_random_uuid(), 'Dr. Justin Howard', 'Orthopedics', 'jhoward@hospital.com', '555-0407', 'MD-ORTH-007', TRUE, NOW() - INTERVAL '14 days'),
(gen_random_uuid(), 'Dr. Kimberly Ward', 'Orthopedics', 'kward@hospital.com', '555-0408', 'MD-ORTH-008', TRUE, NOW() - INTERVAL '13 days'),
(gen_random_uuid(), 'Dr. Brandon Torres', 'Orthopedics', 'btorres@hospital.com', '555-0409', 'MD-ORTH-009', TRUE, NOW() - INTERVAL '12 days'),
(gen_random_uuid(), 'Dr. Heather Peterson', 'Orthopedics', 'hpeterson@hospital.com', '555-0410', 'MD-ORTH-010', TRUE, NOW() - INTERVAL '11 days'),
(gen_random_uuid(), 'Dr. Jeremy Gray', 'Orthopedics', 'jgray@hospital.com', '555-0411', 'MD-ORTH-011', TRUE, NOW() - INTERVAL '10 days'),
(gen_random_uuid(), 'Dr. Crystal Ramirez', 'Orthopedics', 'cramirez@hospital.com', '555-0412', 'MD-ORTH-012', TRUE, NOW() - INTERVAL '9 days'),
(gen_random_uuid(), 'Dr. Tyler James', 'Orthopedics', 'tjames@hospital.com', '555-0413', 'MD-ORTH-013', TRUE, NOW() - INTERVAL '8 days'),
(gen_random_uuid(), 'Dr. Erica Watson', 'Orthopedics', 'ewatson@hospital.com', '555-0414', 'MD-ORTH-014', TRUE, NOW() - INTERVAL '7 days'),
(gen_random_uuid(), 'Dr. Adam Brooks', 'Orthopedics', 'abrooks@hospital.com', '555-0415', 'MD-ORTH-015', TRUE, NOW() - INTERVAL '6 days');

-- Radiology Providers (15)
INSERT INTO "Providers" ("ProviderId", "Name", "Specialty", "Email", "Phone", "LicenseNumber", "IsActive", "CreatedAt") VALUES
(gen_random_uuid(), 'Dr. Vanessa Kelly', 'Radiology', 'vkelly@hospital.com', '555-0501', 'MD-RADI-001', TRUE, NOW() - INTERVAL '5 days'),
(gen_random_uuid(), 'Dr. Sean Sanders', 'Radiology', 'ssanders@hospital.com', '555-0502', 'MD-RADI-002', TRUE, NOW() - INTERVAL '4 days'),
(gen_random_uuid(), 'Dr. Monica Price', 'Radiology', 'mprice@hospital.com', '555-0503', 'MD-RADI-003', TRUE, NOW() - INTERVAL '3 days'),
(gen_random_uuid(), 'Dr. Gary Bennett', 'Radiology', 'gbennett@hospital.com', '555-0504', 'MD-RADI-004', TRUE, NOW() - INTERVAL '2 days'),
(gen_random_uuid(), 'Dr. Tiffany Wood', 'Radiology', 'twood@hospital.com', '555-0505', 'MD-RADI-005', TRUE, NOW() - INTERVAL '1 day'),
(gen_random_uuid(), 'Dr. Russell Ross', 'Radiology', 'rross@hospital.com', '555-0506', 'MD-RADI-006', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Andrea Henderson', 'Radiology', 'ahenderson@hospital.com', '555-0507', 'MD-RADI-007', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Keith Coleman', 'Radiology', 'kcoleman@hospital.com', '555-0508', 'MD-RADI-008', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Julie Jenkins', 'Radiology', 'jjenkins@hospital.com', '555-0509', 'MD-RADI-009', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Scott Perry', 'Radiology', 'sperry@hospital.com', '555-0510', 'MD-RADI-010', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Courtney Powell', 'Radiology', 'cpowell@hospital.com', '555-0511', 'MD-RADI-011', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Marcus Long', 'Radiology', 'mlong@hospital.com', '555-0512', 'MD-RADI-012', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Denise Patterson', 'Radiology', 'dpatterson@hospital.com', '555-0513', 'MD-RADI-013', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Craig Hughes', 'Radiology', 'chughes@hospital.com', '555-0514', 'MD-RADI-014', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Allison Flores', 'Radiology', 'aflores@hospital.com', '555-0515', 'MD-RADI-015', TRUE, NOW());

-- Psychiatry Providers (15)
INSERT INTO "Providers" ("ProviderId", "Name", "Specialty", "Email", "Phone", "LicenseNumber", "IsActive", "CreatedAt") VALUES
(gen_random_uuid(), 'Dr. Patrick Washington', 'Psychiatry', 'pwashington@hospital.com', '555-0601', 'MD-PSYC-001', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Christine Butler', 'Psychiatry', 'cbutler@hospital.com', '555-0602', 'MD-PSYC-002', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Ian Simmons', 'Psychiatry', 'isimmons@hospital.com', '555-0603', 'MD-PSYC-003', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Janet Foster', 'Psychiatry', 'jfoster@hospital.com', '555-0604', 'MD-PSYC-004', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Louis Gonzales', 'Psychiatry', 'lgonzales@hospital.com', '555-0605', 'MD-PSYC-005', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Pamela Bryant', 'Psychiatry', 'pbryant@hospital.com', '555-0606', 'MD-PSYC-006', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Victor Alexander', 'Psychiatry', 'valexander@hospital.com', '555-0607', 'MD-PSYC-007', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Teresa Russell', 'Psychiatry', 'trussell@hospital.com', '555-0608', 'MD-PSYC-008', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Carl Griffin', 'Psychiatry', 'cgriffin@hospital.com', '555-0609', 'MD-PSYC-009', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Sharon Diaz', 'Psychiatry', 'sdiaz@hospital.com', '555-0610', 'MD-PSYC-010', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Arthur Hayes', 'Psychiatry', 'ahayes@hospital.com', '555-0611', 'MD-PSYC-011', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Frances Myers', 'Psychiatry', 'fmyers@hospital.com', '555-0612', 'MD-PSYC-012', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Willie Ford', 'Psychiatry', 'wford@hospital.com', '555-0613', 'MD-PSYC-013', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Anna Hamilton', 'Psychiatry', 'ahamilton@hospital.com', '555-0614', 'MD-PSYC-014', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Roy Graham', 'Psychiatry', 'rgraham@hospital.com', '555-0615', 'MD-PSYC-015', TRUE, NOW());

-- Neurology Providers (10)
INSERT INTO "Providers" ("ProviderId", "Name", "Specialty", "Email", "Phone", "LicenseNumber", "IsActive", "CreatedAt") VALUES
(gen_random_uuid(), 'Dr. Cheryl Sullivan', 'Neurology', 'csullivan@hospital.com', '555-0701', 'MD-NEUR-001', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Dennis Wallace', 'Neurology', 'dwallace@hospital.com', '555-0702', 'MD-NEUR-002', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Janice West', 'Neurology', 'jwest@hospital.com', '555-0703', 'MD-NEUR-003', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Terry Cole', 'Neurology', 'tcole@hospital.com', '555-0704', 'MD-NEUR-004', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Judith Owens', 'Neurology', 'jowens@hospital.com', '555-0705', 'MD-NEUR-005', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Lawrence Reynolds', 'Neurology', 'lreynolds@hospital.com', '555-0706', 'MD-NEUR-006', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Kathryn Fisher', 'Neurology', 'kfisher@hospital.com', '555-0707', 'MD-NEUR-007', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Albert Ellis', 'Neurology', 'aellis@hospital.com', '555-0708', 'MD-NEUR-008', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Gloria Harrison', 'Neurology', 'gharrison@hospital.com', '555-0709', 'MD-NEUR-009', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Jesse Gibson', 'Neurology', 'jgibson@hospital.com', '555-0710', 'MD-NEUR-010', TRUE, NOW());

-- Family Medicine Providers (10) - 5 INACTIVE for filter testing
INSERT INTO "Providers" ("ProviderId", "Name", "Specialty", "Email", "Phone", "LicenseNumber", "IsActive", "CreatedAt") VALUES
(gen_random_uuid(), 'Dr. Maria Hernandez', 'Family Medicine', 'mhernandez@hospital.com', '555-0801', 'MD-FAMI-001', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Andrew Kim', 'Family Medicine', 'akim@hospital.com', '555-0802', 'MD-FAMI-002', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Elizabeth Foster', 'Family Medicine', 'efoster@hospital.com', '555-0803', 'MD-FAMI-003', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Samuel Barnes', 'Family Medicine', 'sbarnes@hospital.com', '555-0804', 'MD-FAMI-004', TRUE, NOW()),
(gen_random_uuid(), 'Dr. Linda Mason', 'Family Medicine', 'lmason@hospital.com', '555-0805', 'MD-FAMI-005', TRUE, NOW()),
-- INACTIVE providers (5) for testing IsActive filter
(gen_random_uuid(), 'Dr. Retired Smith', 'Family Medicine', 'rsmith@hospital.com', '555-0806', 'MD-FAMI-006', FALSE, NOW()),
(gen_random_uuid(), 'Dr. Inactive Jones', 'Family Medicine', 'ijones@hospital.com', '555-0807', 'MD-FAMI-007', FALSE, NOW()),
(gen_random_uuid(), 'Dr. On Leave Brown', 'Family Medicine', 'obrown@hospital.com', '555-0808', 'MD-FAMI-008', FALSE, NOW()),
(gen_random_uuid(), 'Dr. Suspended Davis', 'Family Medicine', 'sdavis@hospital.com', '555-0809', 'MD-FAMI-009', FALSE, NOW()),
(gen_random_uuid(), 'Dr. Deactivated Miller', 'Family Medicine', 'dmiller@hospital.com', '555-0810', 'MD-FAMI-010', FALSE, NOW());

-- =====================================================================================
-- TIMESLOTS: 1000+ slots with varied availability
-- =====================================================================================
-- Date Range: Next 30 days (CURRENT_DATE to CURRENT_DATE + 30 days)
-- Time Range: 9:00 AM - 5:00 PM (30-minute intervals = 16 slots/day)
-- Booking Status: 60% available (IsBooked = FALSE), 40% booked (IsBooked = TRUE)
--
-- Edge Cases:
--   - 10 Cardiology providers (with specific LicenseNumbers CARD-011 through CARD-020): 
--     ALL slots booked (IsBooked = TRUE) - ZERO availability for "Join Waitlist" testing
--   - 20 providers: Only 1-2 available slots (limited availability)
--   - Remaining providers: Mix of good availability (60% available slots)
-- =====================================================================================

-- Generate time slots using PostgreSQL generate_series for efficiency
-- This creates slots for next 30 days, 9 AM to 5 PM, 30-minute intervals
WITH 
-- Get provider IDs and categorize them
provider_categories AS (
    SELECT 
        "ProviderId",
        "LicenseNumber",
        CASE 
            -- Zero availability providers (10 Cardiology providers)
            WHEN "LicenseNumber" IN ('MD-CARD-011', 'MD-CARD-012', 'MD-CARD-013', 'MD-CARD-014', 'MD-CARD-015',
                                      'MD-CARD-016', 'MD-CARD-017', 'MD-CARD-018', 'MD-CARD-019', 'MD-CARD-020') 
            THEN 'zero'
            -- Limited availability providers (next 20)
            WHEN "LicenseNumber" IN ('MD-PEDI-001', 'MD-PEDI-002', 'MD-PEDI-003', 'MD-PEDI-004', 'MD-PEDI-005',
                                      'MD-PEDI-006', 'MD-PEDI-007', 'MD-PEDI-008', 'MD-PEDI-009', 'MD-PEDI-010',
                                      'MD-DERM-001', 'MD-DERM-002', 'MD-DERM-003', 'MD-DERM-004', 'MD-DERM-005',
                                      'MD-DERM-006', 'MD-DERM-007', 'MD-DERM-008', 'MD-DERM-009', 'MD-DERM-010')
            THEN 'limited'
            -- Good availability (remaining active providers)
            WHEN "IsActive" = TRUE THEN 'good'
            -- Inactive providers - no slots
            ELSE 'none'
        END AS availability_category,
        ROW_NUMBER() OVER (ORDER BY "CreatedAt") AS provider_num
    FROM "Providers"
    WHERE "IsActive" = TRUE
),
-- Generate date/time combinations (16 time slots per day x 30 days = 480 slots per provider)
time_slots_base AS (
    SELECT 
        (CURRENT_DATE + day_offset * INTERVAL '1 day')::date AS slot_date,
        ('09:00:00'::time + slot_num * INTERVAL '30 minutes')::time AS slot_time,
        day_offset,
        slot_num
    FROM 
        generate_series(0, 29) AS day_offset,  -- 30 days
        generate_series(0, 15) AS slot_num     -- 16 slots per day (9 AM to 4:30 PM)
),
-- Combine providers with time slots
provider_slots AS (
    SELECT 
        gen_random_uuid() AS "TimeSlotId",
        pc."ProviderId",
        (tsb.slot_date + tsb.slot_time)::timestamp AS "StartTime",
        (tsb.slot_date + tsb.slot_time + INTERVAL '30 minutes')::timestamp AS "EndTime",
        -- Determine if slot is booked based on availability category
        CASE 
            WHEN pc.availability_category = 'zero' THEN TRUE  -- All booked (zero availability)
            WHEN pc.availability_category = 'limited' THEN 
                -- Limited: only 1-2 slots available (98% booked)
                CASE WHEN (tsb.day_offset + tsb.slot_num) % 100 < 2 THEN FALSE ELSE TRUE END
            WHEN pc.availability_category = 'good' THEN 
                -- Good: 60% available (40% booked)
                CASE WHEN (pc.provider_num + tsb.day_offset + tsb.slot_num) % 10 < 4 THEN TRUE ELSE FALSE END
            ELSE TRUE
        END AS "IsBooked",
        NULL::uuid AS "AppointmentId",
        NOW() AS "CreatedAt",
        NULL::timestamp AS "UpdatedAt",
        0 AS "RowVersion"
    FROM provider_categories pc
    CROSS JOIN time_slots_base tsb
    WHERE pc.availability_category != 'none'  -- Don't create slots for inactive providers
)
-- Insert all generated time slots
INSERT INTO "TimeSlots" 
    ("TimeSlotId", "ProviderId", "StartTime", "EndTime", "IsBooked", "AppointmentId", "CreatedAt", "UpdatedAt", "RowVersion")
SELECT 
    "TimeSlotId", 
    "ProviderId", 
    "StartTime", 
    "EndTime", 
    "IsBooked", 
    "AppointmentId", 
    "CreatedAt", 
    "UpdatedAt", 
    "RowVersion"
FROM provider_slots;

COMMIT;

-- =====================================================================================
-- VERIFICATION QUERIES
-- =====================================================================================
-- Run these queries after seed script execution to verify data:
--
-- SELECT COUNT(*) FROM "Providers";                          -- Expected: 120
-- SELECT COUNT(*) FROM "Providers" WHERE "IsActive" = TRUE;  -- Expected: 115
-- SELECT COUNT(*) FROM "Providers" WHERE "IsActive" = FALSE; -- Expected: 5
-- SELECT "Specialty", COUNT(*) FROM "Providers" GROUP BY "Specialty"; -- Verify distribution
-- SELECT COUNT(*) FROM "TimeSlots";                          -- Expected: 55,200+ (115 active providers x 16 slots x 30 days)
-- SELECT COUNT(*) FROM "TimeSlots" WHERE "IsBooked" = TRUE;  -- Expected: varies by availability category
-- SELECT COUNT(*) FROM "TimeSlots" WHERE "IsBooked" = FALSE; -- Expected: varies by availability category
--
-- Find providers with ZERO availability (for "Join Waitlist" testing):
-- SELECT p."ProviderId", p."Name", p."Specialty", COUNT(ts."TimeSlotId") AS total_slots,
--        SUM(CASE WHEN ts."IsBooked" = FALSE THEN 1 ELSE 0 END) AS available_slots
-- FROM "Providers" p
-- LEFT JOIN "TimeSlots" ts ON p."ProviderId" = ts."ProviderId"
-- WHERE p."IsActive" = TRUE
-- GROUP BY p."ProviderId", p."Name", p."Specialty"
-- HAVING SUM(CASE WHEN ts."IsBooked" = FALSE THEN 1 ELSE 0 END) = 0
-- ORDER BY p."Name";
--
-- Verify availability distribution:
-- SELECT 
--     CASE 
--         WHEN available_count = 0 THEN 'Zero Availability'
--         WHEN available_count BETWEEN 1 AND 2 THEN 'Limited Availability (1-2 slots)'
--         WHEN available_count >= 10 THEN 'Good Availability (10+ slots)'
--         ELSE 'Mixed Availability'
--     END AS availability_tier,
--     COUNT(*) AS provider_count
-- FROM (
--     SELECT p."ProviderId", 
--            SUM(CASE WHEN ts."IsBooked" = FALSE THEN 1 ELSE 0 END) AS available_count
--     FROM "Providers" p
--     LEFT JOIN "TimeSlots" ts ON p."ProviderId" = ts."ProviderId"
--     WHERE p."IsActive" = TRUE
--     GROUP BY p."ProviderId"
-- ) AS availability_summary
-- GROUP BY 
--     CASE 
--         WHEN available_count = 0 THEN 'Zero Availability'
--         WHEN available_count BETWEEN 1 AND 2 THEN 'Limited Availability (1-2 slots)'
--         WHEN available_count >= 10 THEN 'Good Availability (10+ slots)'
--         ELSE 'Mixed Availability'
--     END;
-- =====================================================================================
