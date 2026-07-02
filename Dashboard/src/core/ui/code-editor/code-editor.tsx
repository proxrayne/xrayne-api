import Editor, {
  type EditorProps,
  type Monaco,
  type OnChange,
  type OnValidate,
} from "@monaco-editor/react";

import { cn } from "@core/lib/utils";

export type CodeEditorJsonSchema = {
  uri?: string;
  url?: string;
  fileMatch?: string[];
  schema?: Record<string, unknown>;
};

export type CodeEditorProps = {
  value?: string;
  onChange?: OnChange;
  disabled?: boolean;
  schemes?: CodeEditorJsonSchema[];
  schemas?: CodeEditorJsonSchema[];
  language?: string;
  onValidate?: OnValidate;
  className?: string;
  editorClassName?: string;
  height?: EditorProps["height"];
  path?: string;
  options?: EditorProps["options"];
};

const DEFAULT_PATH = "file:///xrayne-editor.json";
const DEFAULT_LANGUAGE = "json";

function CodeEditor({
  value,
  onChange,
  disabled = false,
  schemes,
  schemas,
  language = DEFAULT_LANGUAGE,
  onValidate,
  className,
  editorClassName,
  height = 360,
  path = DEFAULT_PATH,
  options,
}: CodeEditorProps) {
  const jsonSchemas = schemas ?? schemes ?? [];

  return (
    <div
      data-slot="code-editor"
      data-disabled={disabled}
      className={cn(
        "overflow-hidden rounded-2xl border border-border bg-card text-card-foreground shadow-sm ring-1 ring-foreground/5 data-[disabled=true]:opacity-70 dark:ring-foreground/10",
        className,
      )}
    >
      <Editor
        className={cn("min-h-40", editorClassName)}
        height={height}
        language={language}
        path={path}
        value={value}
        theme="xrayne"
        loading={<div className="p-4 text-sm text-muted-foreground">Loading editor...</div>}
        beforeMount={(monaco) => {
          defineXrayneTheme(monaco);
          configureJsonSchemas(monaco, jsonSchemas, path);
        }}
        onChange={onChange}
        onValidate={onValidate}
        options={{
          automaticLayout: true,
          domReadOnly: disabled,
          fontLigatures: true,
          fontSize: 13,
          lineNumbersMinChars: 3,
          minimap: { enabled: false },
          padding: { top: 12, bottom: 12 },
          readOnly: disabled,
          renderLineHighlight: "gutter",
          scrollBeyondLastLine: false,
          tabSize: 2,
          wordWrap: "on",
          ...options,
        }}
      />
    </div>
  );
}

function defineXrayneTheme(monaco: Monaco) {
  monaco.editor.defineTheme("xrayne", {
    base: "vs-dark",
    inherit: true,
    rules: [],
    colors: {
      "editor.background": "#09090b",
      "editor.foreground": "#f4f4f5",
      "editorLineNumber.foreground": "#71717a",
      "editorLineNumber.activeForeground": "#e4e4e7",
      "editorCursor.foreground": "#f4f4f5",
      "editor.selectionBackground": "#3f3f46",
      "editor.lineHighlightBackground": "#18181b",
      "editorGutter.background": "#09090b",
    },
  });
}

function configureJsonSchemas(monaco: Monaco, schemas: CodeEditorJsonSchema[], path: string) {
  if (schemas.length === 0) {
    return;
  }

  monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
    validate: true,
    allowComments: true,
    schemas: schemas.map((item) => ({
      uri: item.uri ?? item.url ?? DEFAULT_PATH,
      fileMatch: item.fileMatch ?? [path],
      schema: item.schema,
    })),
  });
}

export { CodeEditor };
