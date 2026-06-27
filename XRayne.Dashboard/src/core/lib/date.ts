import { Duration } from "date-fns";

export function parseDotnetTimeSpan(value: string): Duration {
  const match = value.match(
    /^(?:(?<days>\d+)\.)?(?<hours>\d{1,2}):(?<minutes>\d{2}):(?<seconds>\d{2})(?:\.\d+)?$/,
  );

  if (!match?.groups) {
    throw new Error("Invalid format string.");
  }

  const days = Number(match.groups.days ?? 0);
  const hours = Number(match.groups.hours);
  const minutes = Number(match.groups.minutes);
  const seconds = Number(match.groups.seconds);

  return {
    days,
    hours,
    minutes,
    seconds,
  };
}
