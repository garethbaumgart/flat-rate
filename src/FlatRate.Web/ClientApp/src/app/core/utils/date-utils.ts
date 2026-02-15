/**
 * Formats a Date as a YYYY-MM-DD string using **local** date components.
 *
 * Unlike `Date.toISOString().split('T')[0]`, this function does not convert
 * to UTC first, so midnight in UTC+ timezones (e.g., South Africa UTC+2)
 * will not shift the date back by one day.
 */
export function formatDateToISO(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
