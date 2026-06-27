import { useLayoutEffect, useState } from "react";

import { IS_SERVER } from "./env";

class FakeStorage implements Storage {
  private store = new Map<string, string>();

  get length(): number {
    return this.store.size;
  }

  clear(): void {
    this.store.clear();
  }

  getItem(key: string): string | null {
    return this.store.get(key) ?? null;
  }

  key(index: number): string | null {
    const keys = Array.from(this.store.keys());

    return keys[index] ?? null;
  }

  removeItem(key: string): void {
    this.store.delete(key);
  }

  setItem(key: string, value: string): void {
    this.store.set(key, value);
  }
}

class ObjectStorage {
  constructor(private readonly storage: Storage) {}

  get length(): number {
    return this.storage.length;
  }

  set<T>(key: string, value: T): void {
    return this.storage.setItem(key, JSON.stringify(value));
  }

  get<T>(key: string): T | null {
    const jsonData = this.storage.getItem(key);
    if (!jsonData) {
      return null;
    }

    return JSON.parse(jsonData);
  }

  remove(key: string): void {
    this.storage.removeItem(key);
  }

  clear(): void {
    this.storage.clear();
  }
}

interface ChangeEvent {
  key: string;
  previous: unknown;
  current: unknown;
  isMainMutator: boolean;
}

interface Listener {
  (event: ChangeEvent): void;
}

class ReactiveLocalStorage extends ObjectStorage {
  private subscribers = new Map<string, Set<Listener>>();

  constructor(storage: Storage) {
    super(storage);

    if (!IS_SERVER) {
      this.subscribeWindow();
    }
  }

  subscribe(key: string, listener: Listener): VoidFunction {
    let keySubscribers = this.subscribers.get(key);
    if (!keySubscribers) {
      keySubscribers = new Set([listener]);
    } else {
      keySubscribers.add(listener);
    }

    this.subscribers.set(key, keySubscribers);

    return () => {
      this.subscribers.get(key)?.delete(listener);
    };
  }

  set<T>(key: string, value: T): void {
    const previous = this.get(key);

    super.set(key, value);
    this.emit({ previous, current: value, key, isMainMutator: true });
  }

  remove(key: string): void {
    const previous = this.get(key);

    super.remove(key);
    this.emit({ previous, key, current: null, isMainMutator: true });
  }

  clear(): void {
    const prevValues = new Map<string, unknown>();
    for (const [key] of this.subscribers) {
      prevValues.set(key, this.get(key));
    }

    super.clear();
    for (const [key, subscribers] of this.subscribers) {
      for (const subscriber of subscribers) {
        subscriber({
          key,
          current: null,
          previous: prevValues.get(key),
          isMainMutator: true,
        });
      }
    }
  }

  useState<T>(key: string): T | null {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    const [data, setData] = useState<T | null>(() => this.get(key));

    // eslint-disable-next-line react-hooks/rules-of-hooks
    useLayoutEffect(() => {
      return this.subscribe(key, ({ current }) => {
        setData(current as T);
      });
    }, [key]);

    return data;
  }

  private emit(event: ChangeEvent): void {
    const keySubscribers = this.subscribers.get(event.key);
    if (!keySubscribers) {
      return;
    }

    for (const subscriber of keySubscribers) {
      subscriber(event);
    }
  }

  private subscribeWindow() {
    window.addEventListener("storage", ({ newValue, oldValue, key }) => {
      this.emit({
        current: newValue && JSON.parse(newValue),
        key: key!,
        previous: oldValue && JSON.parse(oldValue),
        isMainMutator: false,
      });
    });
  }
}

export const lStorage = new ReactiveLocalStorage(IS_SERVER ? new FakeStorage() : localStorage);

export const sStorage = new ObjectStorage(IS_SERVER ? new FakeStorage() : sessionStorage);
