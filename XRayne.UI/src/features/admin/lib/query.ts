import { useMemo } from "react";
import { queryOptions, useQuery } from "@tanstack/react-query";

import { query } from "@core/lib/query";

import { fetchAdminAccount } from "./api";

export const adminAccountQuery = queryOptions({
  queryKey: ["admin-account"],
  queryFn: ({ signal }) => fetchAdminAccount(signal),
  refetchOnWindowFocus: true,
  retry: 1,
});

export function useAdminAccount() {
  const { data, isFetched, error, refetch } = useQuery(adminAccountQuery);

  const permissionGroup = useMemo(() => {
    if (!data?.permissions) {
      return "Unknown";
    }

    if (data.permissions.includes("super_admin")) {
      return "Super admin";
    }

    const contains = (...roles: AdminAccountPermission[]) =>
      roles.some(data.permissions.includes);

    if (contains("manage_admins")) {
      return "Manager";
    }

    if (contains("view_logs")) {
      return "Developer";
    }

    return "Admin";
  }, [data?.permissions]);

  return {
    permissionGroup,
    account: data,
    isLoaded: isFetched,
    error,
    refetch,
  };
}

useAdminAccount.getOrFetch = () => query.fetchQuery(adminAccountQuery);
useAdminAccount.setData = (admin: AdminAccount) =>
  query.setQueryData(adminAccountQuery.queryKey, admin);
