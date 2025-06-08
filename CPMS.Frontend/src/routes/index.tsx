import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import ChargeTagList from "./ChargeTag/ChargeTagList.tsx";
import ChargeTagDetail from "./ChargeTag/ChargeTagDetail.tsx";
import Layout from '../components/layout/Layout';
import ChargePointDetail from "./ChargePoint/ChargePointDetail.tsx";
import ChargePointList from "./ChargePoint/ChargePointList.tsx";
import Home from "./Home/Home.tsx";
import ChargeSessionDetail from "./ChargeSession/ChargeSessionDetail.tsx";
import ChargeSessionList from "./ChargeSession/ChargeSessionList.tsx";
import ChargeLocationList from "./Location/ChargeLocationList.tsx";
import ChargeLocationDetail from "./Location/ChargeLocationDetail.tsx";
import BillingDashboard from "./Billing/BillingDashboard.tsx";
import PricingGroupList from "./Billing/PricingGroupList.tsx";
import PaymentSuccess from "./Billing/PaymentSuccess.tsx";
import PaymentCancel from "./Billing/PaymentCancel.tsx";

const router = createBrowserRouter([
    {
        path: '/',
        element: <Layout />,
        children: [
            { index: true, element: <Home /> },
            { path: 'locations', element: <ChargeLocationList /> },
            { path: 'locations/:id', element: <ChargeLocationDetail /> },
            { path: 'charge-points', element: <ChargePointList /> },
            { path: 'charge-points/:id', element: <ChargePointDetail /> },
            { path: 'charge-tags', element: <ChargeTagList /> },
            { path: 'charge-tags/:id', element: <ChargeTagDetail /> },
            { path: 'charge-sessions', element: <ChargeSessionList /> },
            { path: 'charge-sessions/:id', element: <ChargeSessionDetail /> },
            { path: 'billing', element: <BillingDashboard /> },
            { path: 'billing/pricing-groups', element: <PricingGroupList /> },
            { path: 'payment-success', element: <PaymentSuccess /> },
            { path: 'payment-cancel', element: <PaymentCancel /> },
        ],
    },
]);

const AppRoutes = () => {
    return <RouterProvider router={router} />;
};

export default AppRoutes;