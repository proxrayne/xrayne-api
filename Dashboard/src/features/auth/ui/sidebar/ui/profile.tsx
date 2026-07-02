import { useState } from "react";
import { LogOutIcon } from "lucide-react";

import { cn } from "@core/lib/utils";
import { clearAuthorizationToken } from "@core/api/instance";
import { Avatar, AvatarFallback, AvatarImage } from "@core/ui/avatar";
import { Button } from "@core/ui/button";
import { Spinner } from "@core/ui/spinner";

import { useAdminAccount } from "@features/admin";

interface Props {
  className?: string;
}

function Profile({ className }: Props) {
  const { account, permissionGroup } = useAdminAccount();
  const [isSigningOut, setIsSigningOut] = useState(false);

  return (
    <div className={cn("flex items-center gap-x-3 p-2", className)}>
      <Avatar>
        <AvatarImage
          alt="Blue"
          src="https://heroui-assets.nyc3.cdn.digitaloceanspaces.com/avatars/blue.jpg"
        />
        <AvatarFallback>{account?.username?.slice(2)}</AvatarFallback>
      </Avatar>
      <div className="overflow-hidden">
        <p className="text-md leading-6 font-medium text-ellipsis text-nowrap overflow-hidden">
          {account?.username}
        </p>
        <p className="text-sm/tight text-muted-foreground text-ellipsis text-nowrap overflow-hidden">
          {permissionGroup}
        </p>
      </div>
      <Button
        className="ml-auto -mr-1.5"
        variant="ghost"
        size="icon"
        title="Sign out"
        onClick={() => {
          setIsSigningOut(true);
          clearAuthorizationToken();
        }}
      >
        {isSigningOut ? <Spinner /> : <LogOutIcon />}
      </Button>
    </div>
  );
}

export default Profile;
