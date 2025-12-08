import FingerprintJS from '@fingerprintjs/fingerprintjs';

class FingerprintService {
  private fpPromise = FingerprintJS.load();
  private cachedFingerprint: string | null = null;

  async getFingerprint(): Promise<string> {
    if (this.cachedFingerprint) {
      return this.cachedFingerprint;
    }

    const stored = localStorage.getItem('deviceFingerprint');
    if (stored) {
      this.cachedFingerprint = stored;
      return stored;
    }

    const fp = await this.fpPromise;
    const result = await fp.get();
    const fingerprint = result.visitorId;

    localStorage.setItem('deviceFingerprint', fingerprint);
    this.cachedFingerprint = fingerprint;

    console.log('Generated device fingerprint:', fingerprint);

    return fingerprint;
  }

  getDeviceInfo(): string {
    const ua = navigator.userAgent;
    let browser = 'Unknown';
    let os = 'Unknown';

    if (ua.includes('Chrome')) browser = 'Chrome';
    else if (ua.includes('Firefox')) browser = 'Firefox';
    else if (ua.includes('Safari')) browser = 'Safari';
    else if (ua.includes('Edge')) browser = 'Edge';

    if (ua.includes('Windows')) os = 'Windows';
    else if (ua.includes('Mac')) os = 'macOS';
    else if (ua.includes('Linux')) os = 'Linux';
    else if (ua.includes('Android')) os = 'Android';
    else if (ua.includes('iOS')) os = 'iOS';

    return `${browser} on ${os}`;
  }

  clearFingerprint() {
    localStorage.removeItem('deviceFingerprint');
    this.cachedFingerprint = null;
  }
}

export const fingerprintService = new FingerprintService();
