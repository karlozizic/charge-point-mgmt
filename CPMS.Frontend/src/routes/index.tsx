import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import ChargeTagList from "./ChargeTagList.tsx";
import ChargeTagDetail from "./ChargeTagDetail.tsx";
import Layout from '../components/layout/Layout';
import ChargePointDetail from "./ChargePointDetail.tsx";
import ChargePointList from "./ChargePointList.tsx";
import Home from "./Home.tsx";

const router = createBrowserRouter([
    {
        path: '/',
        element: <Layout />,
        children: [
            { index: true, element: <Home /> },
            { path: 'charge-points', element: <ChargePointList /> },
            { path: 'charge-points/:id', element: <ChargePointDetail /> },
            { path: 'charge-tags', element: <ChargeTagList /> },
            { path: 'charge-tags/:id', element: <ChargeTagDetail /> },
        ],
    },
]);

const AppRoutes = () => {
    return <RouterProvider router={router} />;
};

export default AppRoutes;