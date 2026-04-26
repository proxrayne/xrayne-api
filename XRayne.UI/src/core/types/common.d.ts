declare type Theme = "light" | "dark";

declare interface Create {
  createdAt: Date;
}

declare interface CreateUpdate extends Create {
  updatedAt?: Date;
}

declare type PickByValueExact<T, ValueType> = Pick<
  T,
  {
    [Key in keyof T]-?: [ValueType] extends [T[Key]]
      ? [T[Key]] extends [ValueType]
        ? Key
        : never
      : never;
  }[keyof T]
>;

declare interface Window {
  __TANSTACK_QUERY_CLIENT__: import("@tanstack/query-core").QueryClient;
  __INIT_THEME__: Theme;
}

declare interface AsyncContext extends AppLoadContext {
  serverCookie: import("core/lib/cookie").ServerCookie;
  request: Request;
  params: Params<string>;
}
