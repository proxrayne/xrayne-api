import { Avatar, cn } from "@heroui/react";

import { useAdminAccount } from "@features/admin";

interface Props {
  className?: string;
}

function Profile({ className }: Props) {
  const { account, permissionGroup } = useAdminAccount();

  return (
    <div className={cn("flex items-center gap-x-3 py-5 px-2", className)}>
      <Avatar>
        <Avatar.Image
          alt="Blue"
          src="https://heroui-assets.nyc3.cdn.digitaloceanspaces.com/avatars/blue.jpg"
        />
        <Avatar.Fallback>{account?.username?.slice(2)}</Avatar.Fallback>
      </Avatar>
      <div>
        <p className="text-lg font-medium">{account?.username}</p>
        <p className="text-sm text-muted">{permissionGroup}</p>
      </div>
    </div>
  );
}

export default Profile;
