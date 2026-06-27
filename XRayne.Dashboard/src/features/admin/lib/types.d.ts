declare interface AdminAccount {
  id: string;
  username: string;
  permissions: AdminAccountPermission[];
  createdAt: Date;
  lastLoginAt: Date | null;
}

declare type AdminAccountPermission =
  | "super_admin"
  | "create_users"
  | "edit_users"
  | "delete_users"
  | "reset_traffic"
  | "change_xray_settings"
  | "view_logs"
  | "manage_admins";
