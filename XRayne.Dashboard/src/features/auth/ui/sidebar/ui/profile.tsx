import { cn } from "@core/lib/utils";
import { Avatar, AvatarFallback, AvatarImage } from "@core/ui/avatar";

import { useAdminAccount } from "@features/admin";

interface Props {
  className?: string;
}

function Profile({ className }: Props) {
  const { account, permissionGroup } = useAdminAccount();

  return (
    <div className={cn("flex items-center gap-x-3", className)}>
      <Avatar>
        <AvatarImage
          alt="Blue"
          src="https://heroui-assets.nyc3.cdn.digitaloceanspaces.com/avatars/blue.jpg"
        />
        <AvatarFallback>{account?.username?.slice(2)}</AvatarFallback>
      </Avatar>
      <div className="overflow-hidden">
        <p className="text-md font-medium text-ellipsis text-nowrap overflow-hidden">
          {account?.username}
        </p>
        <p className="text-sm/tight text-muted-foreground text-ellipsis text-nowrap overflow-hidden">{permissionGroup}</p>
      </div>
    </div>
  );
}

export default Profile;
