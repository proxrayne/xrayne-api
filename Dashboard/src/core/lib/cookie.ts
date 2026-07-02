import { parse, serialize, type SerializeOptions, type Cookies } from "cookie";

export interface CookieStorage {
  get(name: string): string | null;
  set(name: string, value: string, options?: SerializeOptions): void;
  getAll(): Cookies;
  delete(name: string): void;
}

export class ClientCookie implements CookieStorage {
  get(name: string): string | null {
    return this.getAll()[name] ?? null;
  }

  set(name: string, value: string, options?: SerializeOptions): void {
    document.cookie = serialize(name, value, options);
  }

  getAll(): Cookies {
    return parse(document.cookie);
  }

  delete(name: string, domain?: string): void {
    const cookie = parse(document.cookie || "");
    if (name in cookie) {
      this.set(name, "", { expires: new Date(0), path: "/", domain });
    }
  }
}

export const cookies = new ClientCookie();
