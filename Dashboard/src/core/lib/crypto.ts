import { Base64 } from "js-base64";
import { EncryptionMethod } from "@libs/xray-config";

interface SeqOptions {
  type?: "default" | "hex";
  hasNumbers?: boolean;
  hasLowercase?: boolean;
  hasUppercase?: boolean;
}

export function getSeq({
  type = "default",
  hasNumbers = true,
  hasLowercase = true,
  hasUppercase = true,
}: SeqOptions = {}) {
  let seq = "";

  switch (type) {
    case "hex":
      seq += "0123456789abcdef";
      break;
    default:
      if (hasNumbers) seq += "0123456789";
      if (hasLowercase) seq += "abcdefghijklmnopqrstuvwxyz";
      if (hasUppercase) seq += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      break;
  }

  return seq;
}

export function randomInteger(min: number, max: number) {
  const range = max - min + 1;
  const randomBuffer = new Uint32Array(1);

  crypto.getRandomValues(randomBuffer);

  return Math.floor((randomBuffer[0] / (0xffffffff + 1)) * range) + min;
}

export function randomSeq(count: number, options = {}) {
  const seq = getSeq(options);
  const seqLength = seq.length;
  const randomValues = new Uint32Array(count);
  crypto.getRandomValues(randomValues);
  return Array.from(randomValues, (v) => seq[v % seqLength]).join("");
}

export function randomShortIds() {
  const lengths = [2, 4, 6, 8, 10, 12, 14, 16].sort(() => Math.random() - 0.5);

  return lengths.map((len) => randomSeq(len, { type: "hex" })).join(",");
}

export function randomLowerAndNum(len: number) {
  return randomSeq(len, { hasUppercase: false });
}

export function randomUUID() {
  if (location.protocol === "https:") {
    return crypto.randomUUID();
  } else {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
      const randomValues = new Uint8Array(1);
      crypto.getRandomValues(randomValues);
      let randomValue = randomValues[0] % 16;
      let calculatedValue = c === "x" ? randomValue : (randomValue & 0x3) | 0x8;
      return calculatedValue.toString(16);
    });
  }
}

export function randomShadowsocksPassword(
  method: EncryptionMethod = EncryptionMethod.Blake3Aes256Gcm,
) {
  const length = method === EncryptionMethod.Blake3Aes128Gcm ? 16 : 32;

  const array = new Uint8Array(length);

  crypto.getRandomValues(array);

  return Base64.encodeURI(String.fromCharCode(...array));
}

export function randomBase64(length = 16) {
  const array = new Uint8Array(length);

  crypto.getRandomValues(array);

  return Base64.encodeURI(String.fromCharCode(...array));
}

export function randomBase32String(length: number = 16) {
  const array = new Uint8Array(length);

  crypto.getRandomValues(array);

  const base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
  let result = "";
  let bits = 0;
  let buffer = 0;

  for (let i = 0; i < array.length; i++) {
    buffer = (buffer << 8) | array[i];
    bits += 8;

    while (bits >= 5) {
      bits -= 5;
      result += base32Chars[(buffer >>> bits) & 0x1f];
    }
  }

  if (bits > 0) {
    result += base32Chars[(buffer << (5 - bits)) & 0x1f];
  }

  return result;
}
