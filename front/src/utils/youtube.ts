/**
 * Extracts YouTube video ID from various YouTube URL formats.
 * NOTE: This function is for DISPLAY PURPOSES ONLY (embedding videos in iframe).
 * It does NOT perform validation - all URL validation happens on the backend.
 * See: back/Validators/YoutubeValidator.cs for validation logic.
 */
export function extractYoutubeId(url: string): string {
  const match = url.match(
    /(?:youtube\.com\/(?:[^/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?/\s]{11})/
  );
  return match ? match[1] : '';
}
