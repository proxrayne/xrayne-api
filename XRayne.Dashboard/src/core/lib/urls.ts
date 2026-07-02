import { createPath, type Path as RouterPath } from "react-router";

export const urls = new (class Urls {
  root() {
    return new Path({ pathname: "/" });
  }

  users() {
    return new Path({ pathname: "/users" });
  }

  nodes() {
    return new Path({ pathname: "/nodes" });
  }

  node(id: string | number) {
    return new Path({ pathname: `/nodes/${id}` });
  }

  hosts() {
    return new Path({ pathname: "/hosts" });
  }

  core() {
    return new Path({ pathname: "/core" });
  }

  settings() {
    return new Path({ pathname: "/settings" });
  }

  signIn(returnUrl?: string) {
    const search = new URLSearchParams();
    if (returnUrl) {
      search.set("return_url", encodeURI(returnUrl));
    }

    return new Path({ pathname: "/sign-in", search: search.toString() });
  }
})();

export class Path implements RouterPath {
  pathname: string;
  search: string;
  hash: string;

  constructor({ hash, pathname, search }: Partial<RouterPath>) {
    this.pathname = pathname ?? "";
    this.search = search ?? "";
    this.hash = hash ?? "";
  }

  toString(): string {
    return createPath(this);
  }

  includes(path: string): boolean {
    const url = this.toString();

    return url.length === 1 ? url === path : path.startsWith(url);
  }

  static join(path: string, ...paths: string[]): string {
    return [path, ...paths].map((path) => path.trim().replace(/^\/|\/$/g, "")).join("/");
  }
}
