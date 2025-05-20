FROM node:18-alpine AS build
WORKDIR /app

COPY CPMS.Frontend/package*.json ./
RUN npm install

COPY CPMS.Frontend/ ./
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY CPMS.Frontend/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]