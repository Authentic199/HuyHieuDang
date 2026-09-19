# syntax=docker/dockerfile:1

# --- Bước 1: dựng gói tĩnh ---
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# Gọi API cùng tên miền; nginx ở bước sau chuyển tiếp sang service backend.
ENV VITE_API_BASE_URL=/api
RUN npm run build

# --- Bước 2: phục vụ bằng nginx ---
FROM nginx:1.27-alpine AS runtime

# Địa chỉ backend, docker-compose có thể ghi đè bằng biến môi trường.
ENV API_UPSTREAM=http://be:8080
ENV NGINX_ENVSUBST_OUTPUT_DIR=/etc/nginx/conf.d

# Ảnh nginx tự chạy envsubst trên templates lúc khởi động.
COPY nginx.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
