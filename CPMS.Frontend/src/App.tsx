import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import AppRoutes from './routes';
import './App.css';

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            retry: 1,
            refetchOnWindowFocus: false,
            staleTime: 300000, // 5 minutes
        },
    },
});

function App() {
    return (
        <QueryClientProvider client={queryClient}>
            <AppRoutes />
        </QueryClientProvider>
    );
}

export default App;