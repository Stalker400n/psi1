import FingerprintJS from '@fingerprintjs/fingerprintjs';

class FingerprintService {
  private fpPromise = FingerprintJS.load();
  private cachedFingerprint: string | null = null;

  async getFingerprint(): Promise<string> {
    // Check if already generated this session
    if (this.cachedFingerprint) {
      return this.cachedFingerprint;
    }

    // Check localStorage
    const stored = localStorage.getItem('deviceFingerprint');
    if (stored) {
      this.cachedFingerprint = stored;
      return stored;
    }

    // Generate new fingerprint
    const fp = await this.fpPromise;
    const result = await fp.get();
    const fingerprint = result.visitorId;

    // Cache it
    localStorage.setItem('deviceFingerprint', fingerprint);
    this.cachedFingerprint = fingerprint;

    console.log('Generated device fingerprint:', fingerprint);

    return fingerprint;
  }

  // Get device info for display
  getDeviceInfo(): string {
    const ua = navigator.userAgent;
    let browser = 'Unknown';
    let os = 'Unknown';

    // Detect browser
    if (ua.includes('Chrome')) browser = 'Chrome';
    else if (ua.includes('Firefox')) browser = 'Firefox';
    else if (ua.includes('Safari')) browser = 'Safari';
    else if (ua.includes('Edge')) browser = 'Edge';

    // Detect OS
    if (ua.includes('Windows')) os = 'Windows';
    else if (ua.includes('Mac')) os = 'macOS';
    else if (ua.includes('Linux')) os = 'Linux';
    else if (ua.includes('Android')) os = 'Android';
    else if (ua.includes('iOS')) os = 'iOS';

    return `${browser} on ${os}`;
  }

  // Clear fingerprint (for testing)
  clearFingerprint() {
    localStorage.removeItem('deviceFingerprint');
    this.cachedFingerprint = null;
  }
}

export const fingerprintService = new FingerprintService();
