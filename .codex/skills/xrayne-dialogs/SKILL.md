---
name: xrayne-dialogs
description: XRayne dialog implementation guidance. Use when Codex creates, reviews, or refactors Dialog, AlertDialog, modal, confirmation, create/edit/delete dialog, or any dialog-triggered UI in Dashboard.
---

# XRayne Dialogs

## Core Rule

Render dialog content state lazily.

For every `Dialog` or `AlertDialog`, keep the trigger/shell component lightweight. Put all dialog content state, forms, mutations, query clients, validation hooks, ids, and child components into a separate content component rendered inside `DialogContent` or `AlertDialogContent`.

Use this shape:

```tsx
function EntityDialog(props: EntityDialogProps) {
  const [open, setOpen] = useState(false);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>{props.trigger}</DialogTrigger>
      <DialogContent>{open && <EntityDialogContent onClose={() => setOpen(false)} />}</DialogContent>
    </Dialog>
  );
}
```

For `AlertDialog`, use the same pattern:

```tsx
<AlertDialogContent>
  {open && <DeleteEntityDialogContent entity={entity} onClose={() => setOpen(false)} />}
</AlertDialogContent>
```

## Implementation Guidance

- Do not create form state, confirmation text state, mutation hooks, query clients, generated ids, or expensive child state in the trigger/shell component.
- Do not mount the content component before the dialog opens.
- Keep close guards in the shell when they control whether the dialog can close.
- Pass only stable props needed by the content component, such as the entity being edited/deleted and `onClose`.
- Reset content state by unmounting the content component on close instead of manually clearing every field in the shell.
- Keep UI text in English unless the route already uses another language.
