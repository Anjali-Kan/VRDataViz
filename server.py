import http.server
import socketserver

class GzipHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        if self.path.endswith('.gz'):
            self.send_header('Content-Encoding', 'gzip')
        super().end_headers()

PORT = 8000
with socketserver.TCPServer(("", PORT), GzipHandler) as httpd:
    print(f"Serving at http://localhost:{PORT}")
    httpd.serve_forever()