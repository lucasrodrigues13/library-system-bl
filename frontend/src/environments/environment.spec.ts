import { environment } from './environment';

describe('environment', () => {
  it('exposes a configurable API base URL', () => {
    expect(environment.apiBaseUrl).toContain('http://localhost:8080');
  });
});
