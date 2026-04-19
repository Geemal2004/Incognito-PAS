export function getProposalDocumentViewUrl(documentUrl?: string | null): string | null {
  if (!documentUrl) {
    return null;
  }

  const trimmedUrl = documentUrl.trim();
  if (!trimmedUrl) {
    return null;
  }

  try {
    const parsed = new URL(trimmedUrl);

    const isCloudinaryPdf =
      parsed.hostname.toLowerCase() === 'res.cloudinary.com' && /\.pdf$/i.test(parsed.pathname);

    // Some Cloudinary accounts deny direct PDF delivery (401). Use page-1 image preview instead.
    if (isCloudinaryPdf && parsed.pathname.includes('/image/upload/')) {
      const previewPath = parsed.pathname
        .replace('/image/upload/', '/image/upload/pg_1,f_png/')
        .replace(/\.pdf$/i, '.png');

      return `${parsed.origin}${previewPath}`;
    }

    return trimmedUrl;
  } catch {
    return trimmedUrl;
  }
}
