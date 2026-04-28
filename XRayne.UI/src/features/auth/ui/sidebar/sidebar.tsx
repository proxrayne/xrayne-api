import { Drawer } from "@heroui/react";

import Navbar from "./ui/navbar";
import Profile from "./ui/profile";
import { useSidebar } from "./lib/hooks";

function Sidebar() {
  const { open, isMobile, setOpen } = useSidebar();

  if (isMobile) {
    return (
      <Drawer isOpen={open} onOpenChange={setOpen}>
        <Drawer.Backdrop>
          <Drawer.Content placement="left">
            <Drawer.Dialog className="px-3 py-0">
              <Drawer.Header>
                <Profile />
              </Drawer.Header>
              <Drawer.Body>
                <Navbar.Top />
              </Drawer.Body>
              <Drawer.Footer>
                <Navbar.Bottom />
              </Drawer.Footer>
            </Drawer.Dialog>
          </Drawer.Content>
        </Drawer.Backdrop>
      </Drawer>
    );
  }

  return (
    <div className="w-70 border-r px-2 flex flex-col sticky top-0 overflow-y-auto">
      <Profile />
      <nav className="h-full flex flex-col justify-between">
        <Navbar.Top />
        <Navbar.Bottom />
      </nav>
    </div>
  );
}

export default Sidebar;
