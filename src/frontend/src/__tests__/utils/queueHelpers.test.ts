import { describe, it, expect } from 'vitest';
import {
  calculateWaitTime,
  reorderQueue,
  filterQueueByProvider,
  generateQueueAnnouncement,
} from '../../utils/queueHelpers';
import type { QueuePatient } from '../../utils/queueHelpers';

function makePatient(overrides: Partial<QueuePatient> = {}): QueuePatient {
  return {
    id: '1',
    patientId: 'p1',
    patientName: 'Test Patient',
    appointmentType: 'Walk-in',
    providerId: 'dr1',
    providerName: 'Dr. Smith',
    arrivalTime: new Date(Date.now() - 30 * 60000).toISOString(),
    isPriority: false,
    position: 1,
    ...overrides,
  };
}

describe('calculateWaitTime', () => {
  it('returns dash for null arrival time', () => {
    expect(calculateWaitTime(null)).toBe('-');
  });

  it('returns minutes for recent arrival', () => {
    const tenMinAgo = new Date(Date.now() - 10 * 60000).toISOString();
    expect(calculateWaitTime(tenMinAgo)).toBe('10 min');
  });

  it('returns hours and minutes for longer waits', () => {
    const ninetyMinAgo = new Date(Date.now() - 90 * 60000).toISOString();
    expect(calculateWaitTime(ninetyMinAgo)).toBe('1 hr 30 min');
  });

  it('returns hours only when minutes are zero', () => {
    const twoHrsAgo = new Date(Date.now() - 120 * 60000).toISOString();
    expect(calculateWaitTime(twoHrsAgo)).toBe('2 hr');
  });

  it('returns dash for future arrival time', () => {
    const future = new Date(Date.now() + 60 * 60000).toISOString();
    expect(calculateWaitTime(future)).toBe('-');
  });

  it('returns "< 1 min" for just arrived', () => {
    const justNow = new Date(Date.now() - 10000).toISOString(); // 10 seconds ago
    expect(calculateWaitTime(justNow)).toBe('< 1 min');
  });
});

describe('reorderQueue', () => {
  it('places priority patients before regular', () => {
    const queue = [
      makePatient({ id: '1', isPriority: false, position: 1 }),
      makePatient({ id: '2', isPriority: true, position: 2 }),
    ];
    const result = reorderQueue(queue);
    expect(result).toHaveLength(2);
    expect(result.at(0)?.id).toBe('2');
    expect(result.at(0)?.position).toBe(1);
    expect(result.at(1)?.id).toBe('1');
    expect(result.at(1)?.position).toBe(2);
  });

  it('sorts by arrival time within same priority', () => {
    const earlier = new Date(Date.now() - 60 * 60000).toISOString();
    const later = new Date(Date.now() - 10 * 60000).toISOString();
    const queue = [
      makePatient({ id: 'late', arrivalTime: later, position: 1 }),
      makePatient({ id: 'early', arrivalTime: earlier, position: 2 }),
    ];
    const result = reorderQueue(queue);
    expect(result.at(0)?.id).toBe('early');
  });

  it('puts null arrival time at the end', () => {
    const queue = [
      makePatient({ id: 'no-arrival', arrivalTime: null, position: 1 }),
      makePatient({ id: 'arrived', position: 2 }),
    ];
    const result = reorderQueue(queue);
    expect(result.at(0)?.id).toBe('arrived');
    expect(result.at(1)?.id).toBe('no-arrival');
  });
});

describe('filterQueueByProvider', () => {
  const queue = [
    makePatient({ id: '1', providerId: 'dr1' }),
    makePatient({ id: '2', providerId: 'dr2' }),
  ];

  it('returns all patients for empty provider filter', () => {
    expect(filterQueueByProvider(queue, '')).toHaveLength(2);
  });

  it('filters by specific provider', () => {
    const result = filterQueueByProvider(queue, 'dr1');
    expect(result).toHaveLength(1);
    expect(result.at(0)?.id).toBe('1');
  });
});

describe('generateQueueAnnouncement', () => {
  it('returns empty message for no patients', () => {
    expect(generateQueueAnnouncement([])).toBe('Queue is empty. No patients waiting.');
  });

  it('returns singular message for one patient', () => {
    expect(generateQueueAnnouncement([makePatient()])).toBe('1 patient in queue.');
  });

  it('returns plural message for multiple patients', () => {
    expect(generateQueueAnnouncement([makePatient(), makePatient()])).toBe('2 patients in queue.');
  });
});
