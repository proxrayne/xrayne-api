import type { MetaArgs, MetaDescriptor } from "react-router";

interface MetaConstructorParams {
  title: string;
  description?: string;
  ogImage?: string;
  robots?: string[];
  canonicalUrl?: string;
  tags?: MetaDescriptor[];
}

type MetaMatch =
  | {
      meta: MetaDescriptor[];
    }
  | undefined;

const PRODUCTION_DOMAIN = import.meta.env.VITE_DOMAIN;
const APP_NAME = "XRayne";

export function constructMetadata<P extends MetaMatch[]>(
  {
    title,
    description,
    ogImage,
    robots = ["index", "follow"],
    canonicalUrl,
    tags = [],
  }: MetaConstructorParams,
  matches?: P,
) {
  const currentTitle = !matches ? title : `${title} | ${APP_NAME}`;
  const meta: MetaDescriptor[] = [
    { title: currentTitle },
    { name: "robots", content: robots.join(",") },
    { name: "mobile-web-app-capable", content: "yes" },
    { name: "apple-mobile-web-app-title", content: APP_NAME },
    {
      property: "og:site_name",
      content: APP_NAME,
    },
    {
      property: "og:type",
      content: "website",
    },
    {
      property: "og:title",
      content: currentTitle,
    },
    {
      name: "twitter:title",
      content: currentTitle,
    },
    {
      name: "twitter:card",
      content: "summary_large_image",
    },
    ...tags,
  ];

  if (ogImage) {
    meta.push({
      property: "og:image",
      content: `${PRODUCTION_DOMAIN}${ogImage}`,
    });
  }

  if (description) {
    meta.push(
      {
        name: "description",
        content: description,
      },
      {
        property: "og:description",
        content: description,
      },
      {
        name: "twitter:description",
        content: description,
      },
    );
  }

  if (canonicalUrl) {
    meta.push({
      tagName: "link",
      rel: "canonical",
      href: `${PRODUCTION_DOMAIN}${canonicalUrl === "/" ? "" : canonicalUrl}`,
    });
  }

  return matches ? merge(meta, matches) : meta;
}

function merge(meta: MetaDescriptor[], parent: MetaMatch[]): MetaDescriptor[] {
  return parent.reduceRight(
    (acc, match) => {
      if (!match) {
        return acc;
      }

      for (const parentMeta of match.meta) {
        const index = acc.findIndex(
          (meta) =>
            ("name" in meta && "name" in parentMeta && meta.name === parentMeta.name) ||
            ("property" in meta &&
              "property" in parentMeta &&
              meta.property === parentMeta.property) ||
            ("title" in meta && "title" in parentMeta) ||
            ("rel" in meta && "rel" in parentMeta),
        );

        if (index == -1) {
          // Parent meta not found in acc, so add it
          acc.push(parentMeta);
        }
      }

      return acc;
    },
    [...meta],
  );
}

export const getMetaById = (matches: MetaArgs["matches"], id: string) =>
  matches.find((match) => match.id === id)?.meta;

export const getParentMeta = (matches: MetaArgs["matches"]) => {
  for (let i = matches.length - 2; i >= 0; i--) {
    const { meta } = matches[i];
    if (meta.length) {
      return meta;
    }
  }
};
