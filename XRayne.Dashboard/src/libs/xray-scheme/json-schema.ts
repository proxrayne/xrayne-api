export type JsonSchema = {
  $id?: string;
  $schema?: string;
  title?: string;
  description?: string;
  type?: string | string[];
  enum?: unknown[];
  const?: unknown;
  default?: unknown;
  deprecated?: boolean;
  properties?: Record<string, JsonSchema>;
  patternProperties?: Record<string, JsonSchema>;
  additionalProperties?: boolean | JsonSchema;
  required?: string[];
  items?: JsonSchema;
  oneOf?: JsonSchema[];
  anyOf?: JsonSchema[];
  allOf?: JsonSchema[];
  examples?: unknown[];
  minimum?: number;
  maximum?: number;
  minLength?: number;
  minItems?: number;
  uniqueItems?: boolean;
  pattern?: string;
};

export type JsonSchemaRegistration = {
  uri: string;
  fileMatch: string[];
  schema: JsonSchema;
};
