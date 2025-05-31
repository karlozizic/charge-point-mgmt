FROM node:18-alpine AS build
WORKDIR /app

COPY CPMS.Frontend/package*.json ./
RUN npm install

COPY CPMS.Frontend/ ./

ARG VITE_MAPBOX_ACCESS_TOKEN
ENV VITE_MAPBOX_ACCESS_TOKEN=$VITE_MAPBOX_ACCESS_TOKEN

RUN echo "Building with VITE_MAPBOX_ACCESS_TOKEN: $VITE_MAPBOX_ACCESS_TOKEN"
RUN printenv | grep VITE

RUN npm run build

RUN ls -la dist/
RUN grep -r "VITE_MAPBOX" dist/ || echo "Token not found in build files"

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY CPMS.Frontend/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]