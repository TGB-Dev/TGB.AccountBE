FROM caddy:2-builder-alpine AS builder

RUN xcaddy build
FROM caddy:2-alpine

COPY --from=builder /usr/bin/caddy /usr/bin/caddy
EXPOSE 443
